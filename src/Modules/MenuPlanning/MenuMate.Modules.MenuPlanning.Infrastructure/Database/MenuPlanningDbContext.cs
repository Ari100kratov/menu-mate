using MenuMate.Contracts.MenuPlanning;
using MenuMate.Modules.MenuPlanning.Application.Abstractions;
using MenuMate.Modules.MenuPlanning.Infrastructure.Database.Entities;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace MenuMate.Modules.MenuPlanning.Infrastructure.Database;

/// <summary>
/// EF Core DbContext модуля MenuPlanning.
/// </summary>
public sealed class MenuPlanningDbContext(DbContextOptions<MenuPlanningDbContext> options)
    : DbContext(options), IMenuPlansUnitOfWork, IMenuPlansReadDbContext
{
    internal DbSet<MenuPlanRecord> MenuPlans => Set<MenuPlanRecord>();

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<MenuPlanResponse>> GetMenuPlansAsync(
        UserId ownerUserId,
        CancellationToken cancellationToken)
    {
        return await ProjectMenuPlans(MenuPlans
                .AsNoTracking()
                .Where(menuPlan => menuPlan.OwnerUserId == ownerUserId)
                .OrderByDescending(menuPlan => menuPlan.StartDate))
            .ToArrayAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<MenuPlanResponse?> GetMenuPlanAsync(
        Guid menuPlanId,
        UserId ownerUserId,
        CancellationToken cancellationToken)
    {
        return await ProjectMenuPlans(MenuPlans.AsNoTracking()
                .Where(menuPlan => menuPlan.Id == menuPlanId && menuPlan.OwnerUserId == ownerUserId))
            .SingleOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<MenuPlanResponse> ProjectMenuPlans(IQueryable<MenuPlanRecord> query) =>
        query.Select(menuPlan => new MenuPlanResponse(
            menuPlan.Id,
            menuPlan.Name,
            menuPlan.StartDate,
            menuPlan.EndDate,
            menuPlan.Items
                .OrderBy(item => item.Date)
                .ThenBy(item => item.MealType)
                .Select(item => new MenuPlanItemResponse(
                    item.Id,
                    item.Date,
                    item.MealType.ToString(),
                    item.RecipeId.HasValue ? item.RecipeId.Value.Value : null,
                    item.RecipeTitle,
                    item.Text,
                    item.Servings,
                    item.Comment))
                .ToArray()));

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(MenuPlanningSchema.Name);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MenuPlanningDbContext).Assembly);
    }
}
