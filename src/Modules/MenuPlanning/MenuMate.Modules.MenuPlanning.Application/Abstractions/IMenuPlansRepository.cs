using MenuMate.Modules.MenuPlanning.Domain.Models;

namespace MenuMate.Modules.MenuPlanning.Application.Abstractions;

internal interface IMenuPlansRepository
{
    Task<MenuPlan?> GetByIdAsync(Guid menuPlanId, CancellationToken cancellationToken);

    Task AddAsync(MenuPlan menuPlan, CancellationToken cancellationToken);

    Task UpdateAsync(MenuPlan menuPlan, CancellationToken cancellationToken);

    Task DeleteAsync(MenuPlan menuPlan, CancellationToken cancellationToken);
}
