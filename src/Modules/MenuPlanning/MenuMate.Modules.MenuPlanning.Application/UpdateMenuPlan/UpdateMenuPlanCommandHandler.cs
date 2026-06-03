using MenuMate.Common.Application;
using MenuMate.Contracts.MenuPlanning;
using MenuMate.Modules.MenuPlanning.Application.Abstractions;
using MenuMate.Modules.MenuPlanning.Domain.Models;
using MenuMate.Modules.MenuPlanning.Domain.ValueObjects;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.MenuPlanning.Application.UpdateMenuPlan;

internal sealed class UpdateMenuPlanCommandHandler(
    IMenuPlansRepository repository,
    IMenuPlansUnitOfWork unitOfWork,
    IUserContext userContext,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateMenuPlanCommand, MenuPlanResponse>
{
    public async Task<Result<MenuPlanResponse>> Handle(
        UpdateMenuPlanCommand command,
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

        Result<MenuPlanDateRange> dateRange = MenuPlanDateRange.Create(
            command.Request.StartDate,
            command.Request.EndDate);

        if (dateRange.IsFailure)
        {
            return Result.Failure<MenuPlanResponse>(dateRange.Error);
        }

        Result updateResult = menuPlan.UpdateDetails(
            command.Request.Name,
            dateRange.Value,
            timeProvider.GetUtcNow());

        if (updateResult.IsFailure)
        {
            return Result.Failure<MenuPlanResponse>(updateResult.Error);
        }

        await repository.UpdateAsync(menuPlan, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MenuPlanMapping.ToResponse(menuPlan);
    }
}
