using MenuMate.Common.Application;

namespace MenuMate.Modules.MenuPlanning.Application.RemoveMenuPlanItem;

internal sealed record RemoveMenuPlanItemCommand(Guid MenuPlanId, Guid ItemId) : ICommand;
