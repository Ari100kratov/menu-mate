using MenuMate.Modules.MenuPlanning.Domain.Models;
using MenuMate.Modules.MenuPlanning.Domain.ValueObjects;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.MenuPlanning.Application.Abstractions;

internal interface IMenuCalendarRepository
{
    Task<IReadOnlyCollection<MealSlot>> GetMealSlotsAsync(UserId ownerUserId, CancellationToken cancellationToken);

    Task<MealSlot?> GetMealSlotByIdAsync(
        UserId ownerUserId,
        Guid mealSlotId,
        CancellationToken cancellationToken);

    Task<bool> MealSlotNameExistsAsync(
        UserId ownerUserId,
        string name,
        Guid? excludedMealSlotId,
        CancellationToken cancellationToken);

    Task<int> GetNextMealSlotSortOrderAsync(UserId ownerUserId, CancellationToken cancellationToken);

    Task AddMealSlotAsync(MealSlot mealSlot, CancellationToken cancellationToken);

    Task UpdateMealSlotAsync(MealSlot mealSlot, CancellationToken cancellationToken);

    Task DeleteMealSlotAsync(MealSlot mealSlot, CancellationToken cancellationToken);

    Task<bool> HasItemsForMealSlotAsync(
        UserId ownerUserId,
        Guid mealSlotId,
        CancellationToken cancellationToken);

    Task<MenuCalendarItem?> GetItemByIdAsync(Guid itemId, CancellationToken cancellationToken);

    Task<int> GetNextItemPositionAsync(
        UserId ownerUserId,
        DateOnly date,
        Guid mealSlotId,
        CancellationToken cancellationToken);

    Task AddItemAsync(MenuCalendarItem item, CancellationToken cancellationToken);

    Task UpdateItemAsync(MenuCalendarItem item, CancellationToken cancellationToken);

    Task DeleteItemAsync(MenuCalendarItem item, CancellationToken cancellationToken);

    Task ClearItemsAsync(
        UserId ownerUserId,
        MenuCalendarDateRange dateRange,
        CancellationToken cancellationToken);
}
