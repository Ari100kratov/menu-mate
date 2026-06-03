using MenuMate.Modules.Recipes.Application.Abstractions;
using MenuMate.Modules.Recipes.Domain.Models;
using MenuMate.Modules.Recipes.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace MenuMate.Modules.Recipes.Infrastructure.Database;

internal sealed class EfRecipesRepository(RecipesDbContext dbContext) : IRecipesRepository
{
    public async Task AddAsync(Recipe recipe, CancellationToken cancellationToken)
    {
        await dbContext.Recipes.AddAsync(RecipeRecord.FromDomain(recipe), cancellationToken);
    }

    public async Task<Recipe?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        RecipeRecord? record = await Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(recipe => recipe.Id == id && !recipe.IsDeleted, cancellationToken);

        return record?.ToDomain();
    }

    public async Task UpdateAsync(Recipe recipe, CancellationToken cancellationToken)
    {
        RecipeRecord? record = await Query()
            .FirstOrDefaultAsync(existing => existing.Id == recipe.Id, cancellationToken);

        if (record is null)
        {
            await AddAsync(recipe, cancellationToken);
            return;
        }

        record.Apply(recipe);
    }

    private IQueryable<RecipeRecord> Query() =>
        dbContext.Recipes
            .Include(recipe => recipe.Ingredients)
            .Include(recipe => recipe.Steps)
            .Include(recipe => recipe.Tags);
}
