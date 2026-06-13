using MenuMate.Modules.MenuPlanning.Application.Abstractions;
using MenuMate.Modules.MenuPlanning.Domain.Models;
using MenuMate.Modules.MenuPlanning.Domain.ValueObjects;
using MenuMate.Modules.MenuPlanning.Infrastructure.Database.Entities;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace MenuMate.Modules.MenuPlanning.Infrastructure.Database;

internal sealed class EfMenuCalendarRepository(MenuPlanningDbContext dbContext) : IMenuCalendarRepository
{
    public async Task<IReadOnlyCollection<MealSlot>> GetMealSlotsAsync(
        UserId ownerUserId,
        CancellationToken cancellationToken)
    {
        MealSlotRecord[] records = await GetMealSlotSettings(ownerUserId)
            .AsNoTracking()
            .OrderBy(mealSlot => mealSlot.SortOrder)
            .ThenBy(mealSlot => mealSlot.Name)
            .ToArrayAsync(cancellationToken);
        return records.Select(mealSlot => mealSlot.ToDomain()).ToArray();
    }

    public async Task<MealSlot?> GetMealSlotByIdAsync(
        UserId ownerUserId,
        Guid mealSlotId,
        CancellationToken cancellationToken) =>
        (await GetMealSlotsAsync(ownerUserId, cancellationToken)).SingleOrDefault(slot => slot.Id == mealSlotId);

    public async Task<bool> MealSlotNameExistsAsync(
        UserId ownerUserId,
        string name,
        Guid? excludedMealSlotId,
        CancellationToken cancellationToken)
    {
        string normalized = name.Trim();
        IReadOnlyCollection<MealSlot> mealSlots = await GetMealSlotsAsync(ownerUserId, cancellationToken);
        return mealSlots.Any(mealSlot =>
            mealSlot.Id != excludedMealSlotId &&
            string.Equals(mealSlot.Name, normalized, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<int> GetNextMealSlotSortOrderAsync(
        UserId ownerUserId,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<MealSlot> mealSlots = await GetMealSlotsAsync(ownerUserId, cancellationToken);
        return mealSlots.Count == 0 ? 0 : mealSlots.Max(mealSlot => mealSlot.SortOrder) + 1;
    }

    public Task AddMealSlotAsync(MealSlot mealSlot, CancellationToken cancellationToken) =>
        dbContext.MealSlots.AddAsync(MealSlotRecord.FromDomain(mealSlot), cancellationToken).AsTask();

    public async Task UpdateMealSlotAsync(MealSlot mealSlot, CancellationToken cancellationToken)
    {
        MealSlotRecord? record = dbContext.MealSlots.Local.SingleOrDefault(existing =>
            existing.OwnerUserId == mealSlot.OwnerUserId && existing.Id == mealSlot.Id);
        record ??= await dbContext.MealSlots
            .SingleOrDefaultAsync(
                existing => existing.OwnerUserId == mealSlot.OwnerUserId && existing.Id == mealSlot.Id,
                cancellationToken);

        if (record is not null)
        {
            record.Apply(mealSlot);
        }
    }

    public async Task DeleteMealSlotAsync(MealSlot mealSlot, CancellationToken cancellationToken)
    {
        MealSlotRecord? record = await dbContext.MealSlots
            .SingleOrDefaultAsync(
                existing => existing.OwnerUserId == mealSlot.OwnerUserId && existing.Id == mealSlot.Id,
                cancellationToken);
        if (record is not null)
        {
            dbContext.MealSlots.Remove(record);
        }
    }

    public Task<bool> HasItemsForMealSlotAsync(
        UserId ownerUserId,
        Guid mealSlotId,
        CancellationToken cancellationToken) =>
        dbContext.MenuCalendarItems
            .AsNoTracking()
            .AnyAsync(
                item => item.OwnerUserId == ownerUserId && item.MealSlotId == mealSlotId,
                cancellationToken);

    public async Task<MenuCalendarItem?> GetItemByIdAsync(Guid itemId, CancellationToken cancellationToken)
    {
        MenuCalendarItemRecord? record = await dbContext.MenuCalendarItems
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == itemId, cancellationToken);
        return record?.ToDomain();
    }

    public async Task<int> GetNextItemPositionAsync(
        UserId ownerUserId,
        DateOnly date,
        Guid mealSlotId,
        CancellationToken cancellationToken)
    {
        int? max = await dbContext.MenuCalendarItems
            .AsNoTracking()
            .Where(item =>
                item.OwnerUserId == ownerUserId &&
                item.Date == date &&
                item.MealSlotId == mealSlotId)
            .Select(item => (int?)item.Position)
            .MaxAsync(cancellationToken);
        return (max ?? -1) + 1;
    }

    public Task AddItemAsync(MenuCalendarItem item, CancellationToken cancellationToken) =>
        dbContext.MenuCalendarItems.AddAsync(MenuCalendarItemRecord.FromDomain(item), cancellationToken).AsTask();

    public async Task UpdateItemAsync(MenuCalendarItem item, CancellationToken cancellationToken)
    {
        MenuCalendarItemRecord? record = await dbContext.MenuCalendarItems
            .SingleOrDefaultAsync(existing => existing.Id == item.Id, cancellationToken);
        if (record is not null)
        {
            record.Apply(item);
        }
    }

    public async Task DeleteItemAsync(MenuCalendarItem item, CancellationToken cancellationToken)
    {
        MenuCalendarItemRecord? record = await dbContext.MenuCalendarItems
            .SingleOrDefaultAsync(existing => existing.Id == item.Id, cancellationToken);
        if (record is not null)
        {
            dbContext.MenuCalendarItems.Remove(record);
        }
    }

    public async Task ClearItemsAsync(
        UserId ownerUserId,
        MenuCalendarDateRange dateRange,
        CancellationToken cancellationToken)
    {
        await dbContext.MenuCalendarItems
            .Where(item =>
                item.OwnerUserId == ownerUserId &&
                item.Date >= dateRange.StartDate &&
                item.Date <= dateRange.EndDate)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private IQueryable<MealSlotRecord> GetMealSlotSettings(UserId ownerUserId) =>
        dbContext.MealSlots.Where(mealSlot => mealSlot.OwnerUserId == ownerUserId);
}
