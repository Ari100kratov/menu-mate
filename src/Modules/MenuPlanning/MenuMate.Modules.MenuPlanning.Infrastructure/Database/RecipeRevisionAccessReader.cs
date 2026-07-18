using MenuMate.Modules.MenuPlanning.Application.Abstractions;
using MenuMate.Modules.MenuPlanning.Infrastructure.Database.Source;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace MenuMate.Modules.MenuPlanning.Infrastructure.Database;

internal sealed class RecipeRevisionAccessReader(MenuPlanningDbContext dbContext)
    : IRecipeRevisionAccessReader
{
    public Task<RecipeRevisionMenuSnapshot?> GetAccessibleAsync(
        UserId userId,
        RecipeId recipeId,
        RecipeRevisionId recipeRevisionId,
        CancellationToken cancellationToken) =>
        dbContext.Set<RecipeRevisionAccessSourceRecord>()
            .AsNoTracking()
            .Where(
                revision =>
                    revision.Id == recipeRevisionId &&
                    revision.RecipeId == recipeId &&
                    dbContext.Set<RecipeAccessSourceRecord>().Any(recipe =>
                        recipe.Id == recipeId &&
                        !recipe.IsDeleted &&
                        (recipe.OwnerUserId == userId || recipe.Visibility == "Public")))
            .Select(revision => new RecipeRevisionMenuSnapshot(revision.Title))
            .SingleOrDefaultAsync(cancellationToken);
}
