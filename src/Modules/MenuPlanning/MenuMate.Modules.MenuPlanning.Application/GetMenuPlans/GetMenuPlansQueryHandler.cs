using MenuMate.Common.Application;
using MenuMate.Contracts.MenuPlanning;
using MenuMate.Modules.MenuPlanning.Application.Abstractions;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.MenuPlanning.Application.GetMenuPlans;

internal sealed class GetMenuPlansQueryHandler(IMenuPlansReadDbContext dbContext, IUserContext userContext)
    : IQueryHandler<GetMenuPlansQuery, IReadOnlyCollection<MenuPlanResponse>>
{
    public async Task<Result<IReadOnlyCollection<MenuPlanResponse>>> Handle(
        GetMenuPlansQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<MenuPlanResponse> menuPlans = await dbContext.GetMenuPlansAsync(
            userContext.UserId,
            cancellationToken);
        return menuPlans.ToArray();
    }
}
