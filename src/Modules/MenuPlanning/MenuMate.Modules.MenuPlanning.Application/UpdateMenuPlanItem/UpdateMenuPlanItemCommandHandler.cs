using MenuMate.Common.Application;
using MenuMate.Contracts.MenuPlanning;
using MenuMate.Modules.MenuPlanning.Application.Abstractions;
using MenuMate.Modules.MenuPlanning.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.MenuPlanning.Application.UpdateMenuPlanItem;

internal sealed class UpdateMenuPlanItemCommandHandler(
    IMenuPlansRepository repository,
    IMenuPlansUnitOfWork unitOfWork,
    IUserContext userContext,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateMenuPlanItemCommand, MenuPlanResponse>
{
    public async Task<Result<MenuPlanResponse>> Handle(
        UpdateMenuPlanItemCommand command,
        CancellationToken cancellationToken)
    {
        MenuPlan? menuPlan = await repository.GetByIdAsync(command.MenuPlanId, cancellationToken);
        if (menuPlan is null)
        {
            return Result.Failure<MenuPlanResponse>(MenuPlanningApplicationErrors.NotFound(command.MenuPlanId));
        }

        if (menuPlan.OwnerUserId != userContext.UserId)
        {
            return Result.Failure<MenuPlanResponse>(MenuPlanningApplicationErrors.AccessDenied);
        }

        Result<MenuPlanItem> item = MenuPlanItemRequestMapper.Map(command.ItemId, command.Request);
        if (item.IsFailure)
        {
            return Result.Failure<MenuPlanResponse>(item.Error);
        }

        Result updateResult = menuPlan.UpdateItem(item.Value, timeProvider.GetUtcNow());
        if (updateResult.IsFailure)
        {
            return Result.Failure<MenuPlanResponse>(updateResult.Error);
        }

        await repository.UpdateAsync(menuPlan, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MenuPlanMapping.ToResponse(menuPlan);
    }
}
