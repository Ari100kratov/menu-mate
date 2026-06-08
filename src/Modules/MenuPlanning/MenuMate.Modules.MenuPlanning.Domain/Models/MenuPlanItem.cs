using MenuMate.Modules.MenuPlanning.Domain.Enums;
using MenuMate.Modules.MenuPlanning.Domain.Errors;
using MenuMate.Modules.MenuPlanning.Domain.ValueObjects;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.MenuPlanning.Domain.Models;

/// <summary>
/// Позиция меню: сохраненный рецепт или произвольный текст.
/// </summary>
public sealed class MenuPlanItem : Entity<Guid>
{
    private MenuPlanItem(
        Guid id,
        DateOnly date,
        MealType mealType,
        MenuServings servings,
        RecipeId? recipeId,
        RecipeRevisionId? recipeRevisionId,
        string? recipeTitle,
        string? text,
        string? comment)
        : base(id)
    {
        Date = date;
        MealType = mealType;
        Servings = servings;
        RecipeId = recipeId;
        RecipeRevisionId = recipeRevisionId;
        RecipeTitle = string.IsNullOrWhiteSpace(recipeTitle) ? null : recipeTitle.Trim();
        Text = text;
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
    }

    /// <summary>
    /// Дата приема пищи.
    /// </summary>
    public DateOnly Date { get; }

    /// <summary>
    /// Тип приема пищи.
    /// </summary>
    public MealType MealType { get; }

    /// <summary>
    /// Количество персон.
    /// </summary>
    public MenuServings Servings { get; }

    /// <summary>
    /// Идентификатор рецепта, если позиция основана на рецепте.
    /// </summary>
    public RecipeId? RecipeId { get; }

    /// <summary>
    /// Immutable recipe content revision pinned by this menu item.
    /// </summary>
    public RecipeRevisionId? RecipeRevisionId { get; }

    /// <summary>
    /// Снимок названия рецепта для пунктов на основе рецепта.
    /// </summary>
    public string? RecipeTitle { get; }

    /// <summary>
    /// Произвольный текст, если позиция не является рецептом.
    /// </summary>
    public string? Text { get; }

    /// <summary>
    /// Комментарий к позиции.
    /// </summary>
    public string? Comment { get; }

    /// <summary>
    /// Возвращает true, если позиция основана на рецепте.
    /// </summary>
    public bool IsRecipeItem => RecipeId.HasValue;

    /// <summary>
    /// Создает позицию меню на основе рецепта.
    /// </summary>
    public static MenuPlanItem ForRecipe(
        Guid id,
        DateOnly date,
        MealType mealType,
        RecipeId recipeId,
        RecipeRevisionId recipeRevisionId,
        MenuServings servings,
        string? comment = null,
        string? recipeTitle = null) =>
        new(id, date, mealType, servings, recipeId, recipeRevisionId, recipeTitle, null, comment);

    /// <summary>
    /// Создает произвольную текстовую позицию меню.
    /// </summary>
    public static Result<MenuPlanItem> ForText(
        Guid id,
        DateOnly date,
        MealType mealType,
        string text,
        MenuServings servings,
        string? comment = null)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result.Failure<MenuPlanItem>(MenuPlanErrors.EmptyTextItem);
        }

        return new MenuPlanItem(id, date, mealType, servings, null, null, null, text.Trim(), comment);
    }

    /// <summary>
    /// Восстанавливает позицию плана меню из persistence-снимка.
    /// </summary>
    public static MenuPlanItem Rehydrate(
        Guid id,
        DateOnly date,
        MealType mealType,
        MenuServings servings,
        RecipeId? recipeId,
        RecipeRevisionId? recipeRevisionId,
        string? recipeTitle,
        string? text,
        string? comment) =>
        new(id, date, mealType, servings, recipeId, recipeRevisionId, recipeTitle, text, comment);
}
