using MenuMate.Common.Application;
using MenuMate.Modules.MenuPlanning.Application.Abstractions;
using MenuMate.Modules.MenuPlanning.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.MenuPlanning.Application.DeleteMenuPlan;

internal sealed class DeleteMenuPlanCommandHandler(
    IMenuPlansRepository repository,
    IMenuPlansUnitOfWork unitOfWork,
    IUserContext userContext)
    : ICommandHandler<DeleteMenuPlanCommand>
{
    public async Task<Result> Handle(DeleteMenuPlanCommand command, CancellationToken cancellationToken)
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

        await repository.DeleteAsync(menuPlan, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
