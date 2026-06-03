using MenuMate.Common.Application;
using MenuMate.Contracts.MenuPlanning;

namespace MenuMate.Modules.MenuPlanning.Application.GetMenuPlanById;

internal sealed record GetMenuPlanByIdQuery(Guid MenuPlanId) : IQuery<MenuPlanResponse>;
