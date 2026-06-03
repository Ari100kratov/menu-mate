using MenuMate.Common.Application;
using MenuMate.Modules.MenuPlanning.Application.Abstractions;
using MenuMate.Modules.MenuPlanning.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.MenuPlanning.Application.RemoveMenuPlanItem;

internal sealed class RemoveMenuPlanItemCommandHandler(
    IMenuPlansRepository repository,
    IMenuPlansUnitOfWork unitOfWork,
    IUserContext userContext,
    TimeProvider timeProvider)
    : ICommandHandler<RemoveMenuPlanItemCommand>
{
    public async Task<Result> Handle(RemoveMenuPlanItemCommand command, CancellationToken cancellationToken)
    {
        MenuPlan? menuPlan = await repository.GetByIdAsync(command.MenuPlanId, cancellationToken);
        if (menuPlan is null)
        {
            return Result.Failure(MenuPlanningApplicationErrors.NotFound(command.MenuPlanId));
        }

        if (menuPlan.OwnerUserId != userContext.UserId)
        {
            return Result.Failure(MenuPlanningApplicationErrors.AccessDenied);
        }

        if (!menuPlan.RemoveItem(command.ItemId, timeProvider.GetUtcNow()))
        {
            return Result.Failure(MenuPlanningApplicationErrors.ItemNotFound(command.ItemId));
        }

        await repository.UpdateAsync(menuPlan, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
