using MenuMate.Contracts.MenuPlanning;
using MenuMate.Modules.MenuPlanning.Application.Abstractions;
using MenuMate.Modules.MenuPlanning.Domain.ValueObjects;
using MenuMate.Modules.MenuPlanning.Infrastructure.Database.Entities;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace MenuMate.Modules.MenuPlanning.Infrastructure.Database;

/// <summary>
/// EF Core DbContext модуля MenuPlanning.
/// </summary>
public sealed class MenuPlanningDbContext(DbContextOptions<MenuPlanningDbContext> options)
    : DbContext(options), IMenuCalendarUnitOfWork, IMenuCalendarReadDbContext
{
    internal DbSet<MealSlotRecord> MealSlots => Set<MealSlotRecord>();

    internal DbSet<MenuCalendarItemRecord> MenuCalendarItems => Set<MenuCalendarItemRecord>();

    /// <inheritdoc />
    public async Task<MenuCalendarResponse> GetCalendarAsync(
        UserId ownerUserId,
        MenuCalendarDateRange dateRange,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dateRange);

        IReadOnlyCollection<MealSlotResponse> mealSlots = await GetMealSlotsAsync(ownerUserId, cancellationToken);
        MenuCalendarItemResponse[] items = await MenuCalendarItems
            .AsNoTracking()
            .Where(item =>
                item.OwnerUserId == ownerUserId &&
                item.Date >= dateRange.StartDate &&
                item.Date <= dateRange.EndDate)
            .OrderBy(item => item.Date)
            .ThenBy(item => item.MealSlotId)
            .ThenBy(item => item.Position)
            .Select(item => new MenuCalendarItemResponse(
                item.Id,
                item.Date,
                item.MealSlotId,
                item.Position,
                item.RecipeId.HasValue ? item.RecipeId.Value.Value : null,
                item.RecipeRevisionId.HasValue ? item.RecipeRevisionId.Value.Value : null,
                item.RecipeTitle,
                item.Text,
                item.Servings,
                item.Comment,
                null))
            .ToArrayAsync(cancellationToken);

        return new MenuCalendarResponse(dateRange.StartDate, dateRange.EndDate, mealSlots, items);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<MealSlotResponse>> GetMealSlotsAsync(
        UserId ownerUserId,
        CancellationToken cancellationToken)
    {
        MealSlotRecord[] records = await MealSlots
            .AsNoTracking()
            .Where(mealSlot => mealSlot.OwnerUserId == ownerUserId)
            .OrderBy(mealSlot => mealSlot.SortOrder)
            .ThenBy(mealSlot => mealSlot.Name)
            .ToArrayAsync(cancellationToken);
        return records
            .Select(mealSlot => new MealSlotResponse(mealSlot.Id, mealSlot.Name, mealSlot.SortOrder))
            .ToArray();
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(MenuPlanningSchema.Name);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MenuPlanningDbContext).Assembly);
    }

}
