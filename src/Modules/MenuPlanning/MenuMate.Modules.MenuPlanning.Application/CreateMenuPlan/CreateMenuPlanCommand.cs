using MenuMate.Common.Application;
using MenuMate.Contracts.MenuPlanning;

namespace MenuMate.Modules.MenuPlanning.Application.CreateMenuPlan;

internal sealed record CreateMenuPlanCommand(CreateMenuPlanRequest Request) : ICommand<MenuPlanResponse>;
