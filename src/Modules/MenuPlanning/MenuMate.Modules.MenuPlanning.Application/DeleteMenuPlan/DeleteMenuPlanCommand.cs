using MenuMate.Common.Application;

namespace MenuMate.Modules.MenuPlanning.Application.DeleteMenuPlan;

internal sealed record DeleteMenuPlanCommand(Guid MenuPlanId) : ICommand;
