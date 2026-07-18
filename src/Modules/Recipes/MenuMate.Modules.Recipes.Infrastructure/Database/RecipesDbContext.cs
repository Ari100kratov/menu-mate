using MenuMate.Contracts.Recipes;
using MenuMate.Modules.Recipes.Application.Abstractions;
using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.Modules.Recipes.Infrastructure.Database.Entities;
using MenuMate.Modules.Recipes.Infrastructure.Database.Source;
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
    internal DbSet<RecipeRevisionTagRecord> RecipeRevisionTags => Set<RecipeRevisionTagRecord>();
    internal DbSet<RecipeMenuItemSourceRecord> RecipeMenuItems => Set<RecipeMenuItemSourceRecord>();

    /// <inheritdoc />
    async Task<RecipeReadModel?> IRecipesReadDbContext.GetRecipeAsync(
        Guid recipeId,
        Guid? revisionId,
        UserId currentUserId,
        CancellationToken cancellationToken)
    {
        Guid userId = currentUserId.Value;
        RecipeAccessProjection? recipe = await Recipes
            .AsNoTracking()
            .Where(item => item.Id == recipeId)
            .Select(item => new RecipeAccessProjection(
                item.Id,
                item.CurrentRevisionId,
                item.OwnerUserId == currentUserId,
                item.Visibility,
                item.IsDeleted,
                item.SourceRecipeId,
                item.SourceRevisionId.HasValue ? item.SourceRevisionId.Value.Value : null,
                item.LibraryEntries
                    .Where(entry => entry.UserId == currentUserId)
                    .Select(entry => (Guid?)entry.SavedRevisionId)
                    .FirstOrDefault()))
            .SingleOrDefaultAsync(cancellationToken);

        if (recipe is null)
        {
            return null;
        }

        bool sourceAccessible = !recipe.IsDeleted &&
            (recipe.IsOwnedByCurrentUser || recipe.Visibility == RecipeVisibility.Public);
        Guid displayedRevisionId;
        if (revisionId.HasValue)
        {
            displayedRevisionId = revisionId.Value;
        }
        else if (sourceAccessible)
        {
            displayedRevisionId = recipe.CurrentRevisionId;
        }
        else
        {
            displayedRevisionId = recipe.IsOwnedByCurrentUser
                ? Guid.Empty
                : recipe.SavedRevisionId ?? Guid.Empty;
        }
        if (displayedRevisionId == Guid.Empty)
        {
            return null;
        }

        bool hasSnapshotGrant = recipe.SavedRevisionId == displayedRevisionId ||
            await RecipeMenuItems
                .AsNoTracking()
                .AnyAsync(
                    item =>
                        item.OwnerUserId == userId &&
                        item.RecipeId == recipeId &&
                        item.RecipeRevisionId == displayedRevisionId,
                    cancellationToken);
        if (!sourceAccessible && !hasSnapshotGrant)
        {
            return null;
        }

        RecipeRevisionProjection? revision = await RecipeRevisions
            .AsNoTracking()
            .Where(item => item.Id == displayedRevisionId && item.RecipeId == recipeId)
            .Select(item => new RecipeRevisionProjection(
                item.Id,
                item.RevisionNumber,
                item.Title,
                item.Description,
                item.Servings,
                item.Category,
                item.TotalTimeMinutes,
                item.ActiveTimeMinutes,
                item.SourceUrl,
                RecipeRevisionTags
                    .Where(tag => tag.RecipeRevisionId == item.Id)
                    .Select(tag => tag.TagId)
                    .ToArray(),
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
        if (revision is null)
        {
            return null;
        }

        RecipeImageProjection[] images = sourceAccessible
            ? await RecipeImages
                .AsNoTracking()
                .Where(image => image.RecipeId == recipeId && !image.IsDeleted)
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
                    image.AltText,
                    image.SourceUrl,
                    image.AuthorName,
                    image.LicenseName,
                    image.LicenseUrl))
                .ToArrayAsync(cancellationToken)
            : [];

        bool isFavorite = recipe.SavedRevisionId.HasValue;
        bool isDisplayedRevisionSaved = recipe.SavedRevisionId == revision.Id;
        string revisionState = GetRevisionState(
            sourceAccessible,
            revision.Id,
            recipe.CurrentRevisionId,
            isDisplayedRevisionSaved);
        var response = new RecipeResponse(
            recipe.Id,
            revision.Id,
            sourceAccessible ? recipe.CurrentRevisionId : null,
            recipe.SavedRevisionId,
            revision.RevisionNumber,
            recipe.IsOwnedByCurrentUser,
            isFavorite,
            isDisplayedRevisionSaved,
            revisionState,
            recipe.SourceRecipeId,
            recipe.SourceRevisionId,
            revision.Title,
            revision.Description,
            revision.Servings,
            revision.Category.ToString(),
            recipe.Visibility.ToString(),
            revision.TotalTimeMinutes,
            revision.ActiveTimeMinutes,
            revision.SourceUrl is null ? null : new Uri(revision.SourceUrl, UriKind.Absolute),
            [],
            images.Select(ToResponse).ToArray(),
            revision.Ingredients,
            revision.Steps);

        return new RecipeReadModel(response, revision.TagIds);
    }

    /// <inheritdoc />
    async Task<RecipeRevisionAccessReadModel?> IRecipesReadDbContext.GetRevisionAccessAsync(
        Guid recipeId,
        Guid revisionId,
        UserId currentUserId,
        CancellationToken cancellationToken)
    {
        bool revisionExists = await RecipeRevisions
            .AsNoTracking()
            .AnyAsync(revision => revision.Id == revisionId && revision.RecipeId == recipeId, cancellationToken);
        if (!revisionExists)
        {
            return null;
        }

        RecipeSourceAccessProjection? recipe = await Recipes
            .AsNoTracking()
            .Where(item => item.Id == recipeId)
            .Select(item => new RecipeSourceAccessProjection(
                !item.IsDeleted &&
                (item.OwnerUserId == currentUserId || item.Visibility == RecipeVisibility.Public),
                item.LibraryEntries.Any(entry =>
                    entry.UserId == currentUserId && entry.SavedRevisionId == revisionId)))
            .SingleOrDefaultAsync(cancellationToken);
        if (recipe is null)
        {
            return null;
        }

        bool hasMenuGrant = await RecipeMenuItems
            .AsNoTracking()
            .AnyAsync(
                item =>
                    item.OwnerUserId == currentUserId.Value &&
                    item.RecipeId == recipeId &&
                    item.RecipeRevisionId == revisionId,
                cancellationToken);

        return recipe.IsSourceAccessible || recipe.HasLibraryGrant || hasMenuGrant
            ? new RecipeRevisionAccessReadModel(recipe.IsSourceAccessible)
            : null;
    }

    /// <inheritdoc />
    async Task<IReadOnlyCollection<RecipeListItemReadModel>> IRecipesReadDbContext.GetRecipesAsync(
        UserId currentUserId,
        bool catalog,
        string? search,
        IReadOnlyCollection<Guid> tagIds,
        RecipeCategory? category,
        bool favoritesOnly,
        bool availableOnly,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        Guid[] distinctTagIds = [.. tagIds.Where(tagId => tagId != Guid.Empty).Distinct()];
        string? searchPattern = string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%";
        RecipeListProjection[] recipes;
        if (catalog)
        {
            IQueryable<RecipeListProjection> catalogQuery = CreateCatalogQuery(
                currentUserId,
                searchPattern,
                distinctTagIds,
                category,
                favoritesOnly);
            recipes = await catalogQuery
                .OrderBy(recipe => recipe.Title)
                .ThenBy(recipe => recipe.Id)
                .Skip(skip)
                .Take(take)
                .ToArrayAsync(cancellationToken);
        }
        else
        {
            IQueryable<RecipeListProjection> ownedQuery = CreateOwnedLibraryQuery(
                currentUserId,
                searchPattern,
                distinctTagIds,
                category,
                favoritesOnly);
            IQueryable<RecipeListProjection> favoriteQuery = CreateFavoriteLibraryQuery(
                currentUserId,
                searchPattern,
                distinctTagIds,
                category,
                availableOnly);
            RecipeListProjection[] ownedRecipes = await ownedQuery.ToArrayAsync(cancellationToken);
            RecipeListProjection[] favoriteRecipes = await favoriteQuery.ToArrayAsync(cancellationToken);
            recipes = [
                .. ownedRecipes
                    .Concat(favoriteRecipes)
                    .OrderBy(recipe => recipe.Title)
                    .ThenBy(recipe => recipe.Id)
                    .Skip(skip)
                    .Take(take),
            ];
        }

        Guid[] revisionIds = [.. recipes.Select(recipe => recipe.RevisionId).Distinct()];
        RevisionTagProjection[] revisionTags = revisionIds.Length == 0
            ? []
            : await RecipeRevisionTags
                .AsNoTracking()
                .Where(tag => revisionIds.Contains(tag.RecipeRevisionId))
                .Select(tag => new RevisionTagProjection(tag.RecipeRevisionId, tag.TagId))
                .ToArrayAsync(cancellationToken);
        ILookup<Guid, Guid> tagIdsByRevision = revisionTags
            .ToLookup(tag => tag.RevisionId, tag => tag.TagId);

        Guid[] accessibleRecipeIds =
        [
            .. recipes
                .Where(recipe => recipe.IsSourceAccessible)
                .Select(recipe => recipe.Id)
                .Distinct(),
        ];
        RecipeListCoverProjection[] covers = accessibleRecipeIds.Length == 0
            ? []
            : await RecipeImages
                .AsNoTracking()
                .Where(image =>
                    accessibleRecipeIds.Contains(image.RecipeId) &&
                    !image.IsDeleted &&
                    image.Scope == RecipeImageScope.Cover)
                .OrderByDescending(image => image.CreatedAt)
                .Select(image => new RecipeListCoverProjection(
                    image.RecipeId,
                    new RecipeImageProjection(
                        image.Id,
                        image.Scope,
                        image.StepNumber,
                        image.BucketName,
                        image.ObjectKey,
                        image.ContentType,
                        image.SizeBytes,
                        image.AltText,
                        image.SourceUrl,
                        image.AuthorName,
                        image.LicenseName,
                        image.LicenseUrl)))
                .ToArrayAsync(cancellationToken);
        IReadOnlyDictionary<Guid, RecipeImageProjection> coverByRecipe = covers
            .GroupBy(cover => cover.RecipeId)
            .ToDictionary(group => group.Key, group => group.First().Image);

        return recipes.Select(recipe => new RecipeListItemReadModel(
            new RecipeListItemResponse(
                recipe.Id,
                recipe.RevisionId,
                recipe.IsSourceAccessible ? recipe.CurrentRevisionId : null,
                recipe.RevisionNumber,
                recipe.IsOwnedByCurrentUser,
                recipe.IsFavorite,
                recipe.IsDisplayedRevisionSaved,
                GetRevisionState(
                    recipe.IsSourceAccessible,
                    recipe.RevisionId,
                    recipe.CurrentRevisionId,
                    recipe.IsDisplayedRevisionSaved),
                recipe.Title,
                recipe.Description,
                recipe.Servings,
                recipe.Category.ToString(),
                recipe.Visibility.ToString(),
                recipe.TotalTimeMinutes,
                recipe.ActiveTimeMinutes,
                [],
                coverByRecipe.TryGetValue(recipe.Id, out RecipeImageProjection? cover)
                    ? ToResponse(cover)
                    : null),
            tagIdsByRevision[recipe.RevisionId].ToArray())).ToArray();
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(RecipesSchema.Name);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RecipesDbContext).Assembly);
    }

    private IQueryable<RecipeListProjection> CreateCatalogQuery(
        UserId currentUserId,
        string? searchPattern,
        Guid[] tagIds,
        RecipeCategory? category,
        bool favoritesOnly)
    {
        var query =
            from recipe in Recipes.AsNoTracking()
            join revision in RecipeRevisions.AsNoTracking()
                on recipe.CurrentRevisionId equals revision.Id
            where !recipe.IsDeleted && recipe.Visibility == RecipeVisibility.Public
            select new { Recipe = recipe, Revision = revision };

        if (favoritesOnly)
        {
            query = query.Where(item =>
                item.Recipe.LibraryEntries.Any(entry => entry.UserId == currentUserId));
        }

        if (searchPattern is not null)
        {
            query = query.Where(item =>
                EF.Functions.ILike(item.Revision.Title, searchPattern) ||
                item.Revision.Description != null &&
                EF.Functions.ILike(item.Revision.Description, searchPattern));
        }

        if (tagIds.Length > 0)
        {
            query = query.Where(item => RecipeRevisionTags.Any(tag =>
                tag.RecipeRevisionId == item.Revision.Id && tagIds.Contains(tag.TagId)));
        }

        if (category.HasValue)
        {
            query = query.Where(item => item.Revision.Category == category.Value);
        }

        return query.Select(item => new RecipeListProjection
        {
            Id = item.Recipe.Id,
            RevisionId = item.Revision.Id,
            CurrentRevisionId = item.Recipe.CurrentRevisionId,
            RevisionNumber = item.Revision.RevisionNumber,
            IsOwnedByCurrentUser = item.Recipe.OwnerUserId == currentUserId,
            IsFavorite = item.Recipe.LibraryEntries.Any(entry => entry.UserId == currentUserId),
            IsDisplayedRevisionSaved = item.Recipe.LibraryEntries.Any(entry =>
                entry.UserId == currentUserId && entry.SavedRevisionId == item.Revision.Id),
            IsSourceAccessible = true,
            Title = item.Revision.Title,
            Description = item.Revision.Description,
            Servings = item.Revision.Servings,
            Category = item.Revision.Category,
            Visibility = item.Recipe.Visibility,
            TotalTimeMinutes = item.Revision.TotalTimeMinutes,
            ActiveTimeMinutes = item.Revision.ActiveTimeMinutes,
        });
    }

    private IQueryable<RecipeListProjection> CreateOwnedLibraryQuery(
        UserId currentUserId,
        string? searchPattern,
        Guid[] tagIds,
        RecipeCategory? category,
        bool favoritesOnly)
    {
        var query =
            from recipe in Recipes.AsNoTracking()
            join revision in RecipeRevisions.AsNoTracking()
                on recipe.CurrentRevisionId equals revision.Id
            where !recipe.IsDeleted && recipe.OwnerUserId == currentUserId
            select new { Recipe = recipe, Revision = revision };

        if (favoritesOnly)
        {
            query = query.Where(item =>
                item.Recipe.LibraryEntries.Any(entry => entry.UserId == currentUserId));
        }

        if (searchPattern is not null)
        {
            query = query.Where(item =>
                EF.Functions.ILike(item.Revision.Title, searchPattern) ||
                item.Revision.Description != null &&
                EF.Functions.ILike(item.Revision.Description, searchPattern));
        }

        if (tagIds.Length > 0)
        {
            query = query.Where(item => RecipeRevisionTags.Any(tag =>
                tag.RecipeRevisionId == item.Revision.Id && tagIds.Contains(tag.TagId)));
        }

        if (category.HasValue)
        {
            query = query.Where(item => item.Revision.Category == category.Value);
        }

        return query.Select(item => new RecipeListProjection
        {
            Id = item.Recipe.Id,
            RevisionId = item.Revision.Id,
            CurrentRevisionId = item.Recipe.CurrentRevisionId,
            RevisionNumber = item.Revision.RevisionNumber,
            IsOwnedByCurrentUser = true,
            IsFavorite = item.Recipe.LibraryEntries.Any(entry => entry.UserId == currentUserId),
            IsDisplayedRevisionSaved = item.Recipe.LibraryEntries.Any(entry =>
                entry.UserId == currentUserId && entry.SavedRevisionId == item.Revision.Id),
            IsSourceAccessible = true,
            Title = item.Revision.Title,
            Description = item.Revision.Description,
            Servings = item.Revision.Servings,
            Category = item.Revision.Category,
            Visibility = item.Recipe.Visibility,
            TotalTimeMinutes = item.Revision.TotalTimeMinutes,
            ActiveTimeMinutes = item.Revision.ActiveTimeMinutes,
        });
    }

    private IQueryable<RecipeListProjection> CreateFavoriteLibraryQuery(
        UserId currentUserId,
        string? searchPattern,
        Guid[] tagIds,
        RecipeCategory? category,
        bool availableOnly)
    {
        var query =
            from entry in RecipeLibraryEntries.AsNoTracking()
            join recipe in Recipes.AsNoTracking() on entry.RecipeId equals recipe.Id
            join revision in RecipeRevisions.AsNoTracking() on entry.SavedRevisionId equals revision.Id
            where entry.UserId == currentUserId && recipe.OwnerUserId != currentUserId
            select new { Recipe = recipe, Revision = revision };

        if (availableOnly)
        {
            query = query.Where(item =>
                !item.Recipe.IsDeleted && item.Recipe.Visibility == RecipeVisibility.Public);
        }

        if (searchPattern is not null)
        {
            query = query.Where(item =>
                EF.Functions.ILike(item.Revision.Title, searchPattern) ||
                item.Revision.Description != null &&
                EF.Functions.ILike(item.Revision.Description, searchPattern));
        }

        if (tagIds.Length > 0)
        {
            query = query.Where(item => RecipeRevisionTags.Any(tag =>
                tag.RecipeRevisionId == item.Revision.Id && tagIds.Contains(tag.TagId)));
        }

        if (category.HasValue)
        {
            query = query.Where(item => item.Revision.Category == category.Value);
        }

        return query.Select(item => new RecipeListProjection
        {
            Id = item.Recipe.Id,
            RevisionId = item.Revision.Id,
            CurrentRevisionId = item.Recipe.CurrentRevisionId,
            RevisionNumber = item.Revision.RevisionNumber,
            IsOwnedByCurrentUser = false,
            IsFavorite = true,
            IsDisplayedRevisionSaved = true,
            IsSourceAccessible = !item.Recipe.IsDeleted &&
                item.Recipe.Visibility == RecipeVisibility.Public,
            Title = item.Revision.Title,
            Description = item.Revision.Description,
            Servings = item.Revision.Servings,
            Category = item.Revision.Category,
            Visibility = item.Recipe.Visibility,
            TotalTimeMinutes = item.Revision.TotalTimeMinutes,
            ActiveTimeMinutes = item.Revision.ActiveTimeMinutes,
        });
    }

    private static string GetRevisionState(
        bool sourceAccessible,
        Guid revisionId,
        Guid currentRevisionId,
        bool isDisplayedRevisionSaved)
    {
        if (!sourceAccessible)
        {
            return "SourceUnavailable";
        }

        if (revisionId == currentRevisionId)
        {
            return "Current";
        }

        return isDisplayedRevisionSaved ? "UpdateAvailable" : "Historical";
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
            null,
            image.SourceUrl is null ? null : new Uri(image.SourceUrl, UriKind.Absolute),
            image.AuthorName,
            image.LicenseName,
            image.LicenseUrl is null ? null : new Uri(image.LicenseUrl, UriKind.Absolute));

    private sealed record RecipeAccessProjection(
        Guid Id,
        Guid CurrentRevisionId,
        bool IsOwnedByCurrentUser,
        RecipeVisibility Visibility,
        bool IsDeleted,
        Guid? SourceRecipeId,
        Guid? SourceRevisionId,
        Guid? SavedRevisionId);

    private sealed record RecipeSourceAccessProjection(bool IsSourceAccessible, bool HasLibraryGrant);

    private sealed record RecipeRevisionProjection(
        Guid Id,
        int RevisionNumber,
        string Title,
        string? Description,
        int Servings,
        RecipeCategory Category,
        int? TotalTimeMinutes,
        int? ActiveTimeMinutes,
        string? SourceUrl,
        IReadOnlyCollection<Guid> TagIds,
        IReadOnlyCollection<IngredientResponse> Ingredients,
        IReadOnlyCollection<PreparationStepResponse> Steps);

    private sealed record RecipeListProjection
    {
        public required Guid Id { get; init; }
        public required Guid RevisionId { get; init; }
        public required Guid CurrentRevisionId { get; init; }
        public required int RevisionNumber { get; init; }
        public required bool IsOwnedByCurrentUser { get; init; }
        public required bool IsFavorite { get; init; }
        public required bool IsDisplayedRevisionSaved { get; init; }
        public required bool IsSourceAccessible { get; init; }
        public required string Title { get; init; }
        public string? Description { get; init; }
        public required int Servings { get; init; }
        public required RecipeCategory Category { get; init; }
        public required RecipeVisibility Visibility { get; init; }
        public int? TotalTimeMinutes { get; init; }
        public int? ActiveTimeMinutes { get; init; }
    }

    private sealed record RevisionTagProjection(Guid RevisionId, Guid TagId);

    private sealed record RecipeListCoverProjection(Guid RecipeId, RecipeImageProjection Image);

    private sealed record RecipeImageProjection(
        Guid Id,
        RecipeImageScope Scope,
        int? StepNumber,
        string BucketName,
        string ObjectKey,
        string ContentType,
        long SizeBytes,
        string? AltText,
        string? SourceUrl,
        string? AuthorName,
        string? LicenseName,
        string? LicenseUrl);
}
