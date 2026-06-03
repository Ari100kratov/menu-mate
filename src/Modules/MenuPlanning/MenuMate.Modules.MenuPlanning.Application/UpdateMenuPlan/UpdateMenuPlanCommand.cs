using MenuMate.Common.Application;
using MenuMate.Contracts.MenuPlanning;

namespace MenuMate.Modules.MenuPlanning.Application.UpdateMenuPlan;

internal sealed record UpdateMenuPlanCommand(Guid MenuPlanId, UpdateMenuPlanRequest Request)
    : ICommand<MenuPlanResponse>;
