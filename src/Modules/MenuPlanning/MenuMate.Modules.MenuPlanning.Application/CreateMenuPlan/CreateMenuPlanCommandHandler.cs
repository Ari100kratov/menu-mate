using MenuMate.Common.Application;
using MenuMate.Contracts.MenuPlanning;
using MenuMate.Modules.MenuPlanning.Application.Abstractions;
using MenuMate.Modules.MenuPlanning.Domain.Models;
using MenuMate.Modules.MenuPlanning.Domain.ValueObjects;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.MenuPlanning.Application.CreateMenuPlan;

internal sealed class CreateMenuPlanCommandHandler(
    IMenuPlansRepository repository,
    IMenuPlansUnitOfWork unitOfWork,
    IUserContext userContext,
    TimeProvider timeProvider)
    : ICommandHandler<CreateMenuPlanCommand, MenuPlanResponse>
{
    public async Task<Result<MenuPlanResponse>> Handle(
        CreateMenuPlanCommand command,
        CancellationToken cancellationToken)
    {
        Result<MenuPlanDateRange> dateRange = MenuPlanDateRange.Create(
            command.Request.StartDate,
            command.Request.EndDate);

        if (dateRange.IsFailure)
        {
            return Result.Failure<MenuPlanResponse>(dateRange.Error);
        }

        Result<MenuPlan> menuPlan = MenuPlan.Create(
            Guid.CreateVersion7(),
            userContext.UserId,
            command.Request.Name,
            dateRange.Value,
            timeProvider.GetUtcNow());

        if (menuPlan.IsFailure)
        {
            return Result.Failure<MenuPlanResponse>(menuPlan.Error);
        }

        await repository.AddAsync(menuPlan.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MenuPlanMapping.ToResponse(menuPlan.Value);
    }
}
