using MenuMate.Common.Application;
using MenuMate.Contracts.MenuPlanning;
using MenuMate.Modules.MenuPlanning.Application.Abstractions;
using MenuMate.Modules.MenuPlanning.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.MenuPlanning.Application.AddMenuPlanItem;

internal sealed class AddMenuPlanItemCommandHandler(
    IMenuPlansRepository repository,
    IRecipeRevisionAccessReader recipeRevisionAccessReader,
    IMenuPlansUnitOfWork unitOfWork,
    IUserContext userContext,
    TimeProvider timeProvider)
    : ICommandHandler<AddMenuPlanItemCommand, MenuPlanResponse>
{
    public async Task<Result<MenuPlanResponse>> Handle(
        AddMenuPlanItemCommand command,
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

        Result<MenuPlanItem> item = MenuPlanItemRequestMapper.Map(Guid.CreateVersion7(), command.Request);
        if (item.IsFailure)
        {
            return Result.Failure<MenuPlanResponse>(item.Error);
        }

        if (item.Value.IsRecipeItem &&
            !await recipeRevisionAccessReader.CanUseAsync(
                userContext.UserId,
                item.Value.RecipeId!.Value,
                item.Value.RecipeRevisionId!.Value,
                cancellationToken))
        {
            return Result.Failure<MenuPlanResponse>(MenuPlanningApplicationErrors.AccessDenied);
        }

        Result addResult = menuPlan.AddItem(item.Value, timeProvider.GetUtcNow());
        if (addResult.IsFailure)
        {
            return Result.Failure<MenuPlanResponse>(addResult.Error);
        }

        await repository.UpdateAsync(menuPlan, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MenuPlanMapping.ToResponse(menuPlan);
    }
}
