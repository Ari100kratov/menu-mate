using MenuMate.Contracts.Recipes;
using MenuMate.Modules.Recipes.Application.Abstractions;
using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.Modules.Recipes.Infrastructure.Database.Entities;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace MenuMate.Modules.Recipes.Infrastructure.Database;

/// <summary>
/// EF Core DbContext модуля Recipes.
/// </summary>
public sealed class RecipesDbContext(DbContextOptions<RecipesDbContext> options)
    : DbContext(options), IRecipesUnitOfWork, IRecipesReadDbContext
{
    internal DbSet<RecipeRecord> Recipes => Set<RecipeRecord>();

    internal DbSet<RecipeImageRecord> RecipeImages => Set<RecipeImageRecord>();

    /// <inheritdoc />
    public async Task<RecipeResponse?> GetRecipeAsync(
        Guid recipeId,
        UserId ownerUserId,
        CancellationToken cancellationToken)
    {
        RecipeDetailsProjection? recipeDetails = await Recipes
            .AsNoTracking()
            .Where(recipe => recipe.Id == recipeId && recipe.OwnerUserId == ownerUserId && !recipe.IsDeleted)
            .Select(recipe => new RecipeDetailsProjection(
                recipe.Id,
                recipe.Title,
                recipe.Description,
                recipe.Servings,
                recipe.IsFavorite,
                recipe.SourceUrl,
                recipe.Images
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
                recipe.Tags
                    .OrderBy(tag => tag.Value)
                    .Select(tag => tag.Value)
                    .ToArray(),
                recipe.Ingredients
                    .OrderBy(ingredient => ingredient.Order)
                    .Select(ingredient => new IngredientResponse(
                        ingredient.ProductName,
                        ingredient.Amount,
                        ingredient.Unit.ToString(),
                        ingredient.Comment,
                        ingredient.IsOptional,
                        ingredient.QuantityKind.ToString(),
                        ingredient.Category.ToString()))
                    .ToArray(),
                recipe.Steps
                    .OrderBy(step => step.Number)
                    .Select(step => new PreparationStepResponse(step.Number, step.Text))
                    .ToArray()))
            .SingleOrDefaultAsync(cancellationToken);

        if (recipeDetails is null)
        {
            return null;
        }

        Uri? sourceUrl = recipeDetails.SourceUrl is null
            ? null
            : new Uri(recipeDetails.SourceUrl, UriKind.Absolute);

        return new RecipeResponse(
            recipeDetails.Id,
            recipeDetails.Title,
            recipeDetails.Description,
            recipeDetails.Servings,
            recipeDetails.IsFavorite,
            sourceUrl,
            recipeDetails.Tags,
            recipeDetails.Images
                .Select(image => new RecipeImageResponse(
                    image.Id,
                    image.Scope.ToString(),
                    image.StepNumber,
                    image.BucketName,
                    image.ObjectKey,
                    image.ContentType,
                    image.SizeBytes,
                    image.AltText,
                    null))
                .ToArray(),
            recipeDetails.Ingredients,
            recipeDetails.Steps);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<RecipeListItemResponse>> GetRecipesAsync(
        UserId ownerUserId,
        string? search,
        string? normalizedTag,
        bool favoritesOnly,
        CancellationToken cancellationToken)
    {
        IQueryable<RecipeRecord> query = Recipes
            .AsNoTracking()
            .Where(recipe => recipe.OwnerUserId == ownerUserId && !recipe.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            string searchPattern = $"%{search.Trim()}%";
            query = query.Where(recipe =>
                EF.Functions.ILike(recipe.Title, searchPattern) ||
                recipe.Description != null && EF.Functions.ILike(recipe.Description, searchPattern));
        }

        if (!string.IsNullOrWhiteSpace(normalizedTag))
        {
            query = query.Where(recipe => recipe.Tags.Any(tag => tag.NormalizedValue == normalizedTag));
        }

        if (favoritesOnly)
        {
            query = query.Where(recipe => recipe.IsFavorite);
        }

        RecipeListItemProjection[] recipes = await query
            .OrderBy(recipe => recipe.Title)
            .Select(recipe => new RecipeListItemProjection(
                recipe.Id,
                recipe.Title,
                recipe.Description,
                recipe.Servings,
                recipe.IsFavorite,
                recipe.Tags
                    .OrderBy(tag => tag.Value)
                    .Select(tag => tag.Value)
                    .ToArray(),
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

        return recipes
            .Select(recipe => new RecipeListItemResponse(
                recipe.Id,
                recipe.Title,
                recipe.Description,
                recipe.Servings,
                recipe.IsFavorite,
                recipe.Tags,
                recipe.CoverImage is null
                    ? null
                    : new RecipeImageResponse(
                        recipe.CoverImage.Id,
                        recipe.CoverImage.Scope.ToString(),
                        recipe.CoverImage.StepNumber,
                        recipe.CoverImage.BucketName,
                        recipe.CoverImage.ObjectKey,
                        recipe.CoverImage.ContentType,
                        recipe.CoverImage.SizeBytes,
                        recipe.CoverImage.AltText,
                        null)))
            .ToArray();
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(RecipesSchema.Name);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RecipesDbContext).Assembly);
    }

    private sealed record RecipeDetailsProjection(
        Guid Id,
        string Title,
        string? Description,
        int Servings,
        bool IsFavorite,
        string? SourceUrl,
        IReadOnlyCollection<RecipeImageProjection> Images,
        IReadOnlyCollection<string> Tags,
        IReadOnlyCollection<IngredientResponse> Ingredients,
        IReadOnlyCollection<PreparationStepResponse> Steps);

    private sealed record RecipeListItemProjection(
        Guid Id,
        string Title,
        string? Description,
        int Servings,
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
