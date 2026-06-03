using MenuMate.Modules.MenuPlanning.Application.Abstractions;
using MenuMate.Modules.MenuPlanning.Domain.Models;
using MenuMate.Modules.MenuPlanning.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace MenuMate.Modules.MenuPlanning.Infrastructure.Database;

internal sealed class EfMenuPlansRepository(MenuPlanningDbContext dbContext) : IMenuPlansRepository
{
    public async Task<MenuPlan?> GetByIdAsync(Guid menuPlanId, CancellationToken cancellationToken)
    {
        MenuPlanRecord? record = await Query()
            .AsNoTracking()
            .SingleOrDefaultAsync(menuPlan => menuPlan.Id == menuPlanId, cancellationToken);

        return record?.ToDomain();
    }

    public async Task AddAsync(MenuPlan menuPlan, CancellationToken cancellationToken)
    {
        await dbContext.MenuPlans.AddAsync(MenuPlanRecord.FromDomain(menuPlan), cancellationToken);
    }

    public async Task UpdateAsync(MenuPlan menuPlan, CancellationToken cancellationToken)
    {
        MenuPlanRecord? record = await Query()
            .SingleOrDefaultAsync(existing => existing.Id == menuPlan.Id, cancellationToken);

        if (record is null)
        {
            await AddAsync(menuPlan, cancellationToken);
            return;
        }

        record.Apply(menuPlan);
    }

    public async Task DeleteAsync(MenuPlan menuPlan, CancellationToken cancellationToken)
    {
        MenuPlanRecord? record = await Query()
            .SingleOrDefaultAsync(existing => existing.Id == menuPlan.Id, cancellationToken);

        if (record is not null)
        {
            dbContext.MenuPlans.Remove(record);
        }
    }

    private IQueryable<MenuPlanRecord> Query() =>
        dbContext.MenuPlans
            .Include(menuPlan => menuPlan.Items);
}
