using MenuMate.Common.Application.Tags;
using MenuMate.Modules.Recipes.Application.Abstractions;
using MenuMate.Modules.Recipes.Domain.Models;
using MenuMate.Modules.Recipes.Domain.ValueObjects;
using MenuMate.Modules.Recipes.Infrastructure.Database.Entities;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace MenuMate.Modules.Recipes.Infrastructure.Database;

internal sealed class EfRecipesRepository(RecipesDbContext dbContext, ITagCatalog tagCatalog) : IRecipesRepository
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

        if (record is null)
        {
            return null;
        }

        Guid[] tagIds = await dbContext.RecipeRevisionTags
            .AsNoTracking()
            .Where(tag => tag.RecipeRevisionId == record.CurrentRevisionId)
            .Select(tag => tag.TagId)
            .ToArrayAsync(cancellationToken);
        IReadOnlyDictionary<Guid, string> tagNames = await tagCatalog.GetNamesAsync(
            tagIds,
            cancellationToken);
        RecipeTag[] tags = [.. tagIds.Select(tagId => CreateTag(tagId, tagNames))];

        return record.ToDomain(tags);
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

        Guid previousRevisionId = record.CurrentRevisionId;
        record.Apply(recipe);

        if (previousRevisionId != recipe.CurrentRevisionId.Value)
        {
            RecipeLibraryEntryRecord? ownerEntry = await dbContext.RecipeLibraryEntries
                .SingleOrDefaultAsync(
                    entry => entry.RecipeId == recipe.Id && entry.UserId == recipe.OwnerUserId,
                    cancellationToken);
            if (ownerEntry is not null)
            {
                ownerEntry.SavedRevisionId = recipe.CurrentRevisionId.Value;
            }
        }
    }

    public async Task SaveToLibraryAsync(
        Guid recipeId,
        RecipeRevisionId recipeRevisionId,
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
                    SavedRevisionId = recipeRevisionId.Value,
                    SavedAt = savedAt
                },
                cancellationToken);

            return;
        }

        entry.SavedRevisionId = recipeRevisionId.Value;
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

    private IQueryable<RecipeRecord> Query() =>
        dbContext.Recipes
            .Include(recipe => recipe.Ingredients)
            .Include(recipe => recipe.Steps)
            .Include(recipe => recipe.Revisions);

    private static RecipeTag CreateTag(Guid tagId, IReadOnlyDictionary<Guid, string> tagNames)
    {
        if (!tagNames.TryGetValue(tagId, out string? tagName))
        {
            throw new InvalidOperationException($"Tag '{tagId}' was not found in the global catalog.");
        }

        Result<RecipeTag> tag = RecipeTag.Create(tagId, tagName);
        return tag.IsSuccess ? tag.Value : throw new DomainException(tag.Error);
    }
}
