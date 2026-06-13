using MenuMate.Common.Application;
using MenuMate.Contracts.MenuPlanning;

namespace MenuMate.Modules.MenuPlanning.Application;

internal sealed record GetMenuCalendarQuery(DateOnly StartDate, DateOnly EndDate)
    : IQuery<MenuCalendarResponse>;

internal sealed record GetMealSlotsQuery : IQuery<IReadOnlyCollection<MealSlotResponse>>;
