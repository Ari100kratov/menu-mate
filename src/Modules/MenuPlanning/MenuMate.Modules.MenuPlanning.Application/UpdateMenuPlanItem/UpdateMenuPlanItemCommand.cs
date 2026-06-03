using MenuMate.Common.Application;
using MenuMate.Contracts.MenuPlanning;

namespace MenuMate.Modules.MenuPlanning.Application.UpdateMenuPlanItem;

internal sealed record UpdateMenuPlanItemCommand(
    Guid MenuPlanId,
    Guid ItemId,
    UpdateMenuPlanItemRequest Request) : ICommand<MenuPlanResponse>;
