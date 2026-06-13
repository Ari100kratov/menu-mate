using MenuMate.Contracts.Recipes;
using MenuMate.Modules.Recipes.Application.Abstractions;
using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.Modules.Recipes.Infrastructure.Database.Entities;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace MenuMate.Modules.Recipes.Infrastructure.Database;

/// <summary>
/// EF Core persistence and read projections for the Recipes module.
/// </summary>
public sealed class RecipesDbContext(DbContextOptions<RecipesDbContext> options)
    : DbContext(options), IRecipesUnitOfWork, IRecipesReadDbContext
{
    internal DbSet<RecipeRecord> Recipes => Set<RecipeRecord>();
    internal DbSet<RecipeImageRecord> RecipeImages => Set<RecipeImageRecord>();
    internal DbSet<RecipeLibraryEntryRecord> RecipeLibraryEntries => Set<RecipeLibraryEntryRecord>();
    internal DbSet<RecipeRevisionRecord> RecipeRevisions => Set<RecipeRevisionRecord>();

    /// <inheritdoc />
    public async Task<RecipeResponse?> GetRecipeAsync(
        Guid recipeId,
        UserId currentUserId,
        CancellationToken cancellationToken)
    {
        RecipeDetailsProjection? recipe = await Recipes
            .AsNoTracking()
            .Where(item =>
                item.Id == recipeId &&
                !item.IsDeleted &&
                (item.OwnerUserId == currentUserId ||
                 item.Visibility == RecipeVisibility.Public))
            .Select(item => new RecipeDetailsProjection(
                item.Id,
                item.CurrentRevisionId.Value,
                item.RevisionNumber,
                item.OwnerUserId == currentUserId,
                item.LibraryEntries.Any(entry => entry.UserId == currentUserId),
                item.SourceRecipeId,
                item.SourceRevisionId.HasValue ? item.SourceRevisionId.Value.Value : null,
                item.Title,
                item.Description,
                item.Servings,
                item.Category,
                item.Visibility,
                item.TotalTimeMinutes,
                item.ActiveTimeMinutes,
                item.LibraryEntries.Any(entry => entry.UserId == currentUserId && entry.IsFavorite),
                item.SourceUrl,
                item.Images
                    .Where(image => !image.IsDeleted)
                    .OrderBy(image => image.Scope)
                    .ThenBy(image => image.StepNumber)
                    .ThenBy(image => image.CreatedAt)
                    .Select(image => new RecipeImageProjection(
                        image.Id,
                        image.Scope,
                        image.StepNumber,
                        image.BucketName,
                        image.ObjectKey,
                        image.ContentType,
                        image.SizeBytes,
                        image.AltText))
                    .ToArray(),
                item.Tags.OrderBy(tag => tag.Value).Select(tag => tag.Value).ToArray(),
                item.Ingredients
                    .OrderBy(ingredient => ingredient.Order)
                    .Select(ingredient => new IngredientResponse(
                        ingredient.IngredientId,
                        ingredient.ProductName,
                        ingredient.Amount,
                        ingredient.Unit.ToString(),
                        ingredient.Comment,
                        ingredient.IsOptional,
                        ingredient.Category.ToString()))
                    .ToArray(),
                item.Steps
                    .OrderBy(step => step.Number)
                    .Select(step => new PreparationStepResponse(step.Number, step.Text))
                    .ToArray()))
            .SingleOrDefaultAsync(cancellationToken);

        if (recipe is null)
        {
            return null;
        }

        return new RecipeResponse(
            recipe.Id,
            recipe.CurrentRevisionId,
            recipe.RevisionNumber,
            recipe.IsOwnedByCurrentUser,
            recipe.IsSaved,
            recipe.SourceRecipeId,
            recipe.SourceRevisionId,
            recipe.Title,
            recipe.Description,
            recipe.Servings,
            recipe.Category.ToString(),
            recipe.Visibility.ToString(),
            recipe.TotalTimeMinutes,
            recipe.ActiveTimeMinutes,
            recipe.IsFavorite,
            recipe.SourceUrl is null ? null : new Uri(recipe.SourceUrl, UriKind.Absolute),
            recipe.Tags,
            recipe.Images.Select(ToResponse).ToArray(),
            recipe.Ingredients,
            recipe.Steps);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<RecipeListItemResponse>> GetRecipesAsync(
        UserId currentUserId,
        bool catalog,
        string? search,
        string? normalizedTag,
        bool favoritesOnly,
        CancellationToken cancellationToken)
    {
        IQueryable<RecipeRecord> query = Recipes.AsNoTracking().Where(recipe => !recipe.IsDeleted);
        query = catalog
            ? query.Where(recipe => recipe.Visibility == RecipeVisibility.Public)
            : query.Where(recipe =>
                recipe.OwnerUserId == currentUserId ||
                recipe.Visibility == RecipeVisibility.Public &&
                recipe.LibraryEntries.Any(entry => entry.UserId == currentUserId));

        if (!string.IsNullOrWhiteSpace(search))
        {
            string pattern = $"%{search.Trim()}%";
            query = query.Where(recipe =>
                EF.Functions.ILike(recipe.Title, pattern) ||
                recipe.Description != null && EF.Functions.ILike(recipe.Description, pattern));
        }

        if (!string.IsNullOrWhiteSpace(normalizedTag))
        {
            query = query.Where(recipe => recipe.Tags.Any(tag => tag.NormalizedValue == normalizedTag));
        }

        if (favoritesOnly)
        {
            query = query.Where(recipe =>
                recipe.LibraryEntries.Any(entry => entry.UserId == currentUserId && entry.IsFavorite));
        }

        RecipeListItemProjection[] recipes = await query
            .OrderBy(recipe => recipe.Title)
            .Select(recipe => new RecipeListItemProjection(
                recipe.Id,
                recipe.CurrentRevisionId.Value,
                recipe.RevisionNumber,
                recipe.OwnerUserId == currentUserId,
                recipe.LibraryEntries.Any(entry => entry.UserId == currentUserId),
                recipe.Title,
                recipe.Description,
                recipe.Servings,
                recipe.Category,
                recipe.Visibility,
                recipe.TotalTimeMinutes,
                recipe.ActiveTimeMinutes,
                recipe.LibraryEntries.Any(entry => entry.UserId == currentUserId && entry.IsFavorite),
                recipe.Tags.OrderBy(tag => tag.Value).Select(tag => tag.Value).ToArray(),
                recipe.Images
                    .Where(image => !image.IsDeleted && image.Scope == RecipeImageScope.Cover)
                    .OrderByDescending(image => image.CreatedAt)
                    .Select(image => new RecipeImageProjection(
                        image.Id,
                        image.Scope,
                        image.StepNumber,
                        image.BucketName,
                        image.ObjectKey,
                        image.ContentType,
                        image.SizeBytes,
                        image.AltText))
                    .FirstOrDefault()))
            .ToArrayAsync(cancellationToken);

        return recipes.Select(recipe => new RecipeListItemResponse(
            recipe.Id,
            recipe.CurrentRevisionId,
            recipe.RevisionNumber,
            recipe.IsOwnedByCurrentUser,
            recipe.IsSaved,
            recipe.Title,
            recipe.Description,
            recipe.Servings,
            recipe.Category.ToString(),
            recipe.Visibility.ToString(),
            recipe.TotalTimeMinutes,
            recipe.ActiveTimeMinutes,
            recipe.IsFavorite,
            recipe.Tags,
            recipe.CoverImage is null ? null : ToResponse(recipe.CoverImage))).ToArray();
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(RecipesSchema.Name);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RecipesDbContext).Assembly);
    }

    private static RecipeImageResponse ToResponse(RecipeImageProjection image) =>
        new(
            image.Id,
            image.Scope.ToString(),
            image.StepNumber,
            image.BucketName,
            image.ObjectKey,
            image.ContentType,
            image.SizeBytes,
            image.AltText,
            null);

    private sealed record RecipeDetailsProjection(
        Guid Id,
        Guid CurrentRevisionId,
        int RevisionNumber,
        bool IsOwnedByCurrentUser,
        bool IsSaved,
        Guid? SourceRecipeId,
        Guid? SourceRevisionId,
        string Title,
        string? Description,
        int Servings,
        RecipeCategory Category,
        RecipeVisibility Visibility,
        int? TotalTimeMinutes,
        int? ActiveTimeMinutes,
        bool IsFavorite,
        string? SourceUrl,
        IReadOnlyCollection<RecipeImageProjection> Images,
        IReadOnlyCollection<string> Tags,
        IReadOnlyCollection<IngredientResponse> Ingredients,
        IReadOnlyCollection<PreparationStepResponse> Steps);

    private sealed record RecipeListItemProjection(
        Guid Id,
        Guid CurrentRevisionId,
        int RevisionNumber,
        bool IsOwnedByCurrentUser,
        bool IsSaved,
        string Title,
        string? Description,
        int Servings,
        RecipeCategory Category,
        RecipeVisibility Visibility,
        int? TotalTimeMinutes,
        int? ActiveTimeMinutes,
        bool IsFavorite,
        IReadOnlyCollection<string> Tags,
        RecipeImageProjection? CoverImage);

    private sealed record RecipeImageProjection(
        Guid Id,
        RecipeImageScope Scope,
        int? StepNumber,
        string BucketName,
        string ObjectKey,
        string ContentType,
        long SizeBytes,
        string? AltText);
}
