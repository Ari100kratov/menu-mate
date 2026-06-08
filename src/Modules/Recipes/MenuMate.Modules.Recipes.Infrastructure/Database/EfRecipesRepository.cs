using MenuMate.Modules.Recipes.Application.Abstractions;
using MenuMate.Modules.Recipes.Domain.Models;
using MenuMate.Modules.Recipes.Infrastructure.Database.Entities;
using MenuMate.SharedKernel.Identifiers;
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

    public async Task SaveToLibraryAsync(
        Guid recipeId,
        UserId userId,
        DateTimeOffset savedAt,
        CancellationToken cancellationToken)
    {
        RecipeLibraryEntryRecord? entry = await dbContext.RecipeLibraryEntries
            .SingleOrDefaultAsync(
                existing => existing.RecipeId == recipeId && existing.UserId == userId,
                cancellationToken);

        if (entry is null)
        {
            await dbContext.RecipeLibraryEntries.AddAsync(
                new RecipeLibraryEntryRecord
                {
                    RecipeId = recipeId,
                    UserId = userId,
                    IsFavorite = false,
                    SavedAt = savedAt
                },
                cancellationToken);
        }
    }

    public async Task RemoveFromLibraryAsync(
        Guid recipeId,
        UserId userId,
        CancellationToken cancellationToken)
    {
        RecipeLibraryEntryRecord? entry = await dbContext.RecipeLibraryEntries
            .SingleOrDefaultAsync(
                existing => existing.RecipeId == recipeId && existing.UserId == userId,
                cancellationToken);

        if (entry is not null)
        {
            dbContext.RecipeLibraryEntries.Remove(entry);
        }
    }

    public async Task SetFavoriteAsync(
        Guid recipeId,
        UserId userId,
        bool isFavorite,
        DateTimeOffset savedAt,
        CancellationToken cancellationToken)
    {
        RecipeLibraryEntryRecord? entry = await dbContext.RecipeLibraryEntries
            .SingleOrDefaultAsync(
                existing => existing.RecipeId == recipeId && existing.UserId == userId,
                cancellationToken);

        if (entry is null)
        {
            entry = new RecipeLibraryEntryRecord
            {
                RecipeId = recipeId,
                UserId = userId,
                SavedAt = savedAt
            };
            await dbContext.RecipeLibraryEntries.AddAsync(entry, cancellationToken);
        }

        entry.IsFavorite = isFavorite;
    }

    private IQueryable<RecipeRecord> Query() =>
        dbContext.Recipes
            .Include(recipe => recipe.Ingredients)
            .Include(recipe => recipe.Steps)
            .Include(recipe => recipe.Tags)
            .Include(recipe => recipe.Revisions);
}
