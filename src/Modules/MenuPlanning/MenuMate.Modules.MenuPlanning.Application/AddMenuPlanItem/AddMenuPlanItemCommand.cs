using MenuMate.Common.Application;
using MenuMate.Contracts.MenuPlanning;

namespace MenuMate.Modules.MenuPlanning.Application.AddMenuPlanItem;

internal sealed record AddMenuPlanItemCommand(Guid MenuPlanId, CreateMenuPlanItemRequest Request)
    : ICommand<MenuPlanResponse>;
