using MenuMate.Common.Application;
using MenuMate.Contracts.MenuPlanning;

namespace MenuMate.Modules.MenuPlanning.Application.GetMenuPlans;

internal sealed record GetMenuPlansQuery : IQuery<IReadOnlyCollection<MenuPlanResponse>>;
