using MenuMate.Common.Application;
using MenuMate.Contracts.MenuPlanning;
using MenuMate.Modules.MenuPlanning.Application.Abstractions;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.MenuPlanning.Application.GetMenuPlanById;

internal sealed class GetMenuPlanByIdQueryHandler(IMenuPlansReadDbContext dbContext, IUserContext userContext)
    : IQueryHandler<GetMenuPlanByIdQuery, MenuPlanResponse>
{
    public async Task<Result<MenuPlanResponse>> Handle(
        GetMenuPlanByIdQuery query,
        CancellationToken cancellationToken)
    {
        MenuPlanResponse? menuPlan = await dbContext.GetMenuPlanAsync(
            query.MenuPlanId,
            userContext.UserId,
            cancellationToken);
        return menuPlan is null
            ? Result.Failure<MenuPlanResponse>(MenuPlanningApplicationErrors.NotFound(query.MenuPlanId))
            : menuPlan;
    }
}
