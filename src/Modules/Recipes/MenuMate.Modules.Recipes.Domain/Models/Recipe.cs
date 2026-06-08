using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.Modules.Recipes.Domain.ValueObjects;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.Recipes.Domain.Models;

/// <summary>
/// Editable current projection of an authored recipe.
/// </summary>
public sealed class Recipe : Entity<Guid>
{
    private readonly List<RecipeIngredient> _ingredients = [];
    private readonly List<PreparationStep> _steps = [];
    private readonly List<RecipeTag> _tags = [];

    private Recipe(
        Guid id,
        UserId ownerUserId,
        RecipeTitle title,
        Servings servings,
        RecipeCategory category,
        RecipeVisibility visibility,
        RecipeRevisionId currentRevisionId,
        int revisionNumber,
        Guid? sourceRecipeId,
        RecipeRevisionId? sourceRevisionId,
        DateTimeOffset now)
        : base(id)
    {
        OwnerUserId = ownerUserId;
        Title = title;
        Servings = servings;
        Category = category;
        Visibility = visibility;
        CurrentRevisionId = currentRevisionId;
        RevisionNumber = revisionNumber;
        SourceRecipeId = sourceRecipeId;
        SourceRevisionId = sourceRevisionId;
        CreatedAt = now;
        UpdatedAt = now;
    }

    /// <summary>Gets the user who owns and can edit the recipe.</summary>
    public UserId OwnerUserId { get; }

    /// <summary>Gets the current title.</summary>
    public RecipeTitle Title { get; private set; }

    /// <summary>Gets the current description.</summary>
    public string? Description { get; private set; }

    /// <summary>Gets the base serving count.</summary>
    public Servings Servings { get; private set; }

    /// <summary>Gets the primary recipe category.</summary>
    public RecipeCategory Category { get; private set; }

    /// <summary>Gets the read visibility.</summary>
    public RecipeVisibility Visibility { get; private set; }

    /// <summary>Gets the current immutable revision.</summary>
    public RecipeRevisionId CurrentRevisionId { get; private set; }

    /// <summary>Gets the current revision number.</summary>
    public int RevisionNumber { get; private set; }

    /// <summary>Gets the source recipe for a copied recipe.</summary>
    public Guid? SourceRecipeId { get; }

    /// <summary>Gets the exact source revision for a copied recipe.</summary>
    public RecipeRevisionId? SourceRevisionId { get; }

    /// <summary>Gets the total cooking time in minutes.</summary>
    public int? TotalTimeMinutes { get; private set; }

    /// <summary>Gets the active cooking time in minutes.</summary>
    public int? ActiveTimeMinutes { get; private set; }

    /// <summary>Gets the optional external source URL.</summary>
    public Uri? SourceUrl { get; private set; }

    /// <summary>Gets the creation time.</summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>Gets the last content update time.</summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Gets whether the recipe is soft deleted.</summary>
    public bool IsDeleted { get; private set; }

    /// <summary>Gets the current ingredients.</summary>
    public IReadOnlyCollection<RecipeIngredient> Ingredients => _ingredients.AsReadOnly();

    /// <summary>Gets the current preparation steps.</summary>
    public IReadOnlyCollection<PreparationStep> Steps => _steps.AsReadOnly();

    /// <summary>Gets the current tags.</summary>
    public IReadOnlyCollection<RecipeTag> Tags => _tags.AsReadOnly();

    /// <summary>Creates a new owned recipe with its first revision identity.</summary>
    public static Recipe Create(
        Guid id,
        UserId ownerUserId,
        RecipeTitle title,
        Servings servings,
        RecipeCategory category,
        RecipeVisibility visibility,
        DateTimeOffset now,
        Guid? sourceRecipeId = null,
        RecipeRevisionId? sourceRevisionId = null) =>
        new(
            id,
            ownerUserId,
            title,
            servings,
            category,
            visibility,
            RecipeRevisionId.Create(),
            revisionNumber: 1,
            sourceRecipeId,
            sourceRevisionId,
            now);

    /// <summary>Rehydrates the editable current projection from persistence.</summary>
    public static Recipe Rehydrate(
        Guid id,
        UserId ownerUserId,
        RecipeTitle title,
        Servings servings,
        RecipeCategory category,
        RecipeVisibility visibility,
        RecipeRevisionId currentRevisionId,
        int revisionNumber,
        Guid? sourceRecipeId,
        RecipeRevisionId? sourceRevisionId,
        int? totalTimeMinutes,
        int? activeTimeMinutes,
        string? description,
        Uri? sourceUrl,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        bool isDeleted,
        IEnumerable<RecipeIngredient> ingredients,
        IEnumerable<PreparationStep> steps,
        IEnumerable<RecipeTag> tags)
    {
        ArgumentNullException.ThrowIfNull(ingredients);
        ArgumentNullException.ThrowIfNull(steps);
        ArgumentNullException.ThrowIfNull(tags);

        var recipe = new Recipe(
            id,
            ownerUserId,
            title,
            servings,
            category,
            visibility,
            currentRevisionId,
            revisionNumber,
            sourceRecipeId,
            sourceRevisionId,
            createdAt)
        {
            TotalTimeMinutes = totalTimeMinutes,
            ActiveTimeMinutes = activeTimeMinutes,
            Description = description,
            SourceUrl = sourceUrl,
            UpdatedAt = updatedAt,
            IsDeleted = isDeleted
        };

        recipe._ingredients.AddRange(ingredients);
        recipe._steps.AddRange(steps.OrderBy(step => step.Number));
        recipe._tags.AddRange(tags);

        return recipe;
    }

    /// <summary>Updates the scalar content fields.</summary>
    public void UpdateDetails(
        RecipeTitle title,
        Servings servings,
        RecipeCategory category,
        RecipeVisibility visibility,
        int? totalTimeMinutes,
        int? activeTimeMinutes,
        string? description,
        Uri? sourceUrl,
        DateTimeOffset now)
    {
        Title = title;
        Servings = servings;
        Category = category;
        Visibility = visibility;
        TotalTimeMinutes = totalTimeMinutes;
        ActiveTimeMinutes = activeTimeMinutes;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        SourceUrl = sourceUrl;
        UpdatedAt = now;
    }

    /// <summary>Replaces all current ingredients.</summary>
    public void ReplaceIngredients(IEnumerable<RecipeIngredient> ingredients, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(ingredients);
        _ingredients.Clear();
        _ingredients.AddRange(ingredients);
        UpdatedAt = now;
    }

    /// <summary>Replaces all current preparation steps.</summary>
    public void ReplaceSteps(IEnumerable<PreparationStep> steps, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(steps);
        _steps.Clear();
        _steps.AddRange(steps.OrderBy(step => step.Number));
        UpdatedAt = now;
    }

    /// <summary>Replaces all current tags.</summary>
    public void ReplaceTags(IEnumerable<RecipeTag> tags, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(tags);
        _tags.Clear();
        HashSet<string> normalizedValues = [];
        _tags.AddRange(tags.Where(tag => normalizedValues.Add(tag.NormalizedValue)));
        UpdatedAt = now;
    }

    /// <summary>Adds a tag when it does not already exist.</summary>
    public void AddTag(RecipeTag tag, DateTimeOffset now)
    {
        if (_tags.Any(existing => existing.NormalizedValue == tag.NormalizedValue))
        {
            return;
        }

        _tags.Add(tag);
        UpdatedAt = now;
    }

    /// <summary>Removes a tag by normalized value.</summary>
    public void RemoveTag(string normalizedTagName, DateTimeOffset now)
    {
        _tags.RemoveAll(tag => tag.NormalizedValue == normalizedTagName);
        UpdatedAt = now;
    }

    /// <summary>Assigns the next immutable revision identity to the current content.</summary>
    public void PublishRevision(DateTimeOffset now)
    {
        CurrentRevisionId = RecipeRevisionId.Create();
        RevisionNumber++;
        UpdatedAt = now;
    }

    /// <summary>Scales ingredients without modifying the recipe.</summary>
    public IReadOnlyCollection<RecipeIngredient> ScaleIngredientsFor(Servings targetServings)
    {
        decimal factor = (decimal)targetServings.Value / Servings.Value;
        return _ingredients.Select(ingredient => ingredient.Scale(factor)).ToArray();
    }

    /// <summary>Soft deletes the recipe.</summary>
    public void Delete(DateTimeOffset now)
    {
        IsDeleted = true;
        UpdatedAt = now;
    }
}
