using MenuMate.Modules.Recipes.Domain.ValueObjects;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.Recipes.Domain.Models;

/// <summary>
/// Рецепт с ингредиентами, шагами приготовления и тегами.
/// </summary>
public sealed class Recipe : Entity<Guid>
{
    private readonly List<RecipeIngredient> _ingredients = [];
    private readonly List<PreparationStep> _steps = [];
    private readonly List<RecipeTag> _tags = [];

    private Recipe(Guid id, UserId ownerUserId, RecipeTitle title, Servings servings, DateTimeOffset now)
        : base(id)
    {
        OwnerUserId = ownerUserId;
        Title = title;
        Servings = servings;
        CreatedAt = now;
        UpdatedAt = now;
    }

    /// <summary>
    /// Пользователь, которому принадлежит рецепт.
    /// </summary>
    public UserId OwnerUserId { get; }

    /// <summary>
    /// Название рецепта.
    /// </summary>
    public RecipeTitle Title { get; private set; }

    /// <summary>
    /// Краткое описание.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Количество персон в исходном рецепте.
    /// </summary>
    public Servings Servings { get; private set; }

    /// <summary>
    /// Признак избранного.
    /// </summary>
    public bool IsFavorite { get; private set; }

    /// <summary>
    /// URL источника, если рецепт импортирован из интернета.
    /// </summary>
    public Uri? SourceUrl { get; private set; }

    /// <summary>
    /// Момент создания.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Момент последнего изменения.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// Признак мягкого удаления.
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// Ингредиенты рецепта.
    /// </summary>
    public IReadOnlyCollection<RecipeIngredient> Ingredients => _ingredients.AsReadOnly();

    /// <summary>
    /// Шаги приготовления.
    /// </summary>
    public IReadOnlyCollection<PreparationStep> Steps => _steps.AsReadOnly();

    /// <summary>
    /// Теги рецепта.
    /// </summary>
    public IReadOnlyCollection<RecipeTag> Tags => _tags.AsReadOnly();

    /// <summary>
    /// Создает рецепт.
    /// </summary>
    public static Recipe Create(Guid id, UserId ownerUserId, RecipeTitle title, Servings servings, DateTimeOffset now) =>
        new(id, ownerUserId, title, servings, now);

    /// <summary>
    /// Восстанавливает рецепт из persistence-снимка.
    /// </summary>
    public static Recipe Rehydrate(
        Guid id,
        UserId ownerUserId,
        RecipeTitle title,
        Servings servings,
        string? description,
        bool isFavorite,
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

        var recipe = new Recipe(id, ownerUserId, title, servings, createdAt)
        {
            Description = description,
            IsFavorite = isFavorite,
            SourceUrl = sourceUrl,
            UpdatedAt = updatedAt,
            IsDeleted = isDeleted
        };

        recipe._ingredients.AddRange(ingredients);
        recipe._steps.AddRange(steps.OrderBy(step => step.Number));
        recipe._tags.AddRange(tags);

        return recipe;
    }

    /// <summary>
    /// Обновляет основные поля рецепта.
    /// </summary>
    public void UpdateDetails(
        RecipeTitle title,
        Servings servings,
        string? description,
        Uri? sourceUrl,
        DateTimeOffset now)
    {
        Title = title;
        Servings = servings;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        SourceUrl = sourceUrl;
        UpdatedAt = now;
    }

    /// <summary>
    /// Заменяет ингредиенты рецепта.
    /// </summary>
    public void ReplaceIngredients(IEnumerable<RecipeIngredient> ingredients, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(ingredients);

        _ingredients.Clear();
        _ingredients.AddRange(ingredients);
        UpdatedAt = now;
    }

    /// <summary>
    /// Заменяет шаги приготовления.
    /// </summary>
    public void ReplaceSteps(IEnumerable<PreparationStep> steps, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(steps);

        _steps.Clear();
        _steps.AddRange(steps.OrderBy(step => step.Number));
        UpdatedAt = now;
    }

    /// <summary>
    /// Добавляет тег, если тега с такой нормализованной формой еще нет.
    /// </summary>
    public void AddTag(RecipeTag tag, DateTimeOffset now)
    {
        if (_tags.Any(existing => existing.NormalizedValue == tag.NormalizedValue))
        {
            return;
        }

        _tags.Add(tag);
        UpdatedAt = now;
    }

    /// <summary>
    /// Удаляет тег по нормализованному названию.
    /// </summary>
    public void RemoveTag(string normalizedTagName, DateTimeOffset now)
    {
        _tags.RemoveAll(tag => tag.NormalizedValue == normalizedTagName);
        UpdatedAt = now;
    }

    /// <summary>
    /// Заменяет набор тегов рецепта с дедупликацией по нормализованной форме.
    /// </summary>
    public void ReplaceTags(IEnumerable<RecipeTag> tags, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(tags);

        _tags.Clear();
        HashSet<string> normalizedValues = [];
        foreach (RecipeTag tag in tags.Where(tag => normalizedValues.Add(tag.NormalizedValue)))
        {
            _tags.Add(tag);
        }

        UpdatedAt = now;
    }

    /// <summary>
    /// Добавляет рецепт в избранное.
    /// </summary>
    public void MarkAsFavorite(DateTimeOffset now)
    {
        IsFavorite = true;
        UpdatedAt = now;
    }

    /// <summary>
    /// Убирает рецепт из избранного.
    /// </summary>
    public void UnmarkAsFavorite(DateTimeOffset now)
    {
        IsFavorite = false;
        UpdatedAt = now;
    }

    /// <summary>
    /// Масштабирует ингредиенты под нужное количество персон без изменения исходного рецепта.
    /// </summary>
    public IReadOnlyCollection<RecipeIngredient> ScaleIngredientsFor(Servings targetServings)
    {
        decimal factor = (decimal)targetServings.Value / Servings.Value;
        return _ingredients.Select(ingredient => ingredient.Scale(factor)).ToArray();
    }

    /// <summary>
    /// Выполняет мягкое удаление рецепта.
    /// </summary>
    public void Delete(DateTimeOffset now)
    {
        IsDeleted = true;
        UpdatedAt = now;
    }
}
