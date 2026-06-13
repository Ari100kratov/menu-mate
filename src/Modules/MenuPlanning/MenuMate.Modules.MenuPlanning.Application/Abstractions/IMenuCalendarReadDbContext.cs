using MenuMate.Contracts.MenuPlanning;
using MenuMate.Modules.MenuPlanning.Domain.ValueObjects;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.MenuPlanning.Application.Abstractions;

/// <summary>
/// Контракт чтения календаря меню через EF-проекции.
/// </summary>
internal interface IMenuCalendarReadDbContext
{
    Task<MenuCalendarResponse> GetCalendarAsync(
        UserId ownerUserId,
        MenuCalendarDateRange dateRange,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<MealSlotResponse>> GetMealSlotsAsync(
        UserId ownerUserId,
        CancellationToken cancellationToken);
}
