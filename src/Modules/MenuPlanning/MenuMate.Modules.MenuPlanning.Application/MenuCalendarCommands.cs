using MenuMate.Common.Application;
using MenuMate.Contracts.MenuPlanning;

namespace MenuMate.Modules.MenuPlanning.Application;

internal sealed record AddMenuCalendarItemCommand(CreateMenuCalendarItemRequest Request)
    : ICommand<MenuCalendarItemResponse>;

internal sealed record UpdateMenuCalendarItemCommand(Guid ItemId, UpdateMenuCalendarItemRequest Request)
    : ICommand<MenuCalendarItemResponse>;

internal sealed record RemoveMenuCalendarItemCommand(Guid ItemId) : ICommand;

internal sealed record ClearMenuCalendarCommand(DateOnly StartDate, DateOnly EndDate) : ICommand;

internal sealed record CreateMealSlotCommand(CreateMealSlotRequest Request)
    : ICommand<IReadOnlyCollection<MealSlotResponse>>;

internal sealed record UpdateMealSlotCommand(Guid MealSlotId, UpdateMealSlotRequest Request)
    : ICommand<IReadOnlyCollection<MealSlotResponse>>;

internal sealed record DeleteMealSlotCommand(Guid MealSlotId) : ICommand<IReadOnlyCollection<MealSlotResponse>>;

internal sealed record ReorderMealSlotsCommand(ReorderMealSlotsRequest Request)
    : ICommand<IReadOnlyCollection<MealSlotResponse>>;
