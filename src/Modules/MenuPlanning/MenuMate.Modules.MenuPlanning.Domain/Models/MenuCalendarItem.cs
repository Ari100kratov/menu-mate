using MenuMate.Modules.MenuPlanning.Domain.Errors;
using MenuMate.Modules.MenuPlanning.Domain.ValueObjects;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.MenuPlanning.Domain.Models;

/// <summary>
/// Позиция календаря меню: сохраненный рецепт или произвольный текст.
/// </summary>
public sealed class MenuCalendarItem : Entity<Guid>
{
    private MenuCalendarItem(
        Guid id,
        UserId ownerUserId,
        DateOnly date,
        Guid mealSlotId,
        int position,
        MenuServings servings,
        RecipeId? recipeId,
        RecipeRevisionId? recipeRevisionId,
        string? recipeTitle,
        string? text,
        string? comment,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
        : base(id)
    {
        OwnerUserId = ownerUserId;
        Date = date;
        MealSlotId = mealSlotId;
        Position = position;
        Servings = servings;
        RecipeId = recipeId;
        RecipeRevisionId = recipeRevisionId;
        RecipeTitle = string.IsNullOrWhiteSpace(recipeTitle) ? null : recipeTitle.Trim();
        Text = string.IsNullOrWhiteSpace(text) ? null : text.Trim();
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    /// <summary>
    /// Пользователь, которому принадлежит позиция меню.
    /// </summary>
    public UserId OwnerUserId { get; }

    /// <summary>
    /// Дата приема пищи.
    /// </summary>
    public DateOnly Date { get; private set; }

    /// <summary>
    /// Настраиваемый прием пищи.
    /// </summary>
    public Guid MealSlotId { get; private set; }

    /// <summary>
    /// Порядок внутри приема пищи.
    /// </summary>
    public int Position { get; private set; }

    /// <summary>
    /// Количество порций.
    /// </summary>
    public MenuServings Servings { get; private set; }

    /// <summary>
    /// Идентификатор рецепта, если позиция основана на рецепте.
    /// </summary>
    public RecipeId? RecipeId { get; private set; }

    /// <summary>
    /// Immutable recipe content revision pinned by this menu item.
    /// </summary>
    public RecipeRevisionId? RecipeRevisionId { get; private set; }

    /// <summary>
    /// Снимок названия рецепта.
    /// </summary>
    public string? RecipeTitle { get; private set; }

    /// <summary>
    /// Произвольный текст.
    /// </summary>
    public string? Text { get; private set; }

    /// <summary>
    /// Комментарий к позиции.
    /// </summary>
    public string? Comment { get; private set; }

    /// <summary>
    /// Момент создания.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Момент последнего изменения.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// Возвращает true, если позиция основана на рецепте.
    /// </summary>
    public bool IsRecipeItem => RecipeId.HasValue;

    /// <summary>
    /// Создает позицию меню на основе рецепта.
    /// </summary>
    public static Result<MenuCalendarItem> ForRecipe(
        Guid id,
        UserId ownerUserId,
        DateOnly date,
        Guid mealSlotId,
        int position,
        RecipeId recipeId,
        RecipeRevisionId recipeRevisionId,
        MenuServings servings,
        DateTimeOffset now,
        string? recipeTitle = null,
        string? comment = null)
    {
        Result placement = ValidatePlacement(mealSlotId, position);
        return placement.IsFailure
            ? Result.Failure<MenuCalendarItem>(placement.Error)
            : new MenuCalendarItem(
                id,
                ownerUserId,
                date,
                mealSlotId,
                position,
                servings,
                recipeId,
                recipeRevisionId,
                recipeTitle,
                null,
                comment,
                now,
                now);
    }

    /// <summary>
    /// Создает произвольную текстовую позицию меню.
    /// </summary>
    public static Result<MenuCalendarItem> ForText(
        Guid id,
        UserId ownerUserId,
        DateOnly date,
        Guid mealSlotId,
        int position,
        string text,
        MenuServings servings,
        DateTimeOffset now,
        string? comment = null)
    {
        Result placement = ValidatePlacement(mealSlotId, position);
        if (placement.IsFailure)
        {
            return Result.Failure<MenuCalendarItem>(placement.Error);
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return Result.Failure<MenuCalendarItem>(MenuCalendarErrors.EmptyTextItem);
        }

        return new MenuCalendarItem(
            id,
            ownerUserId,
            date,
            mealSlotId,
            position,
            servings,
            null,
            null,
            null,
            text,
            comment,
            now,
            now);
    }

    /// <summary>
    /// Восстанавливает позицию из persistence-снимка.
    /// </summary>
    public static MenuCalendarItem Rehydrate(
        Guid id,
        UserId ownerUserId,
        DateOnly date,
        Guid mealSlotId,
        int position,
        MenuServings servings,
        RecipeId? recipeId,
        RecipeRevisionId? recipeRevisionId,
        string? recipeTitle,
        string? text,
        string? comment,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt) =>
        new(
            id,
            ownerUserId,
            date,
            mealSlotId,
            position,
            servings,
            recipeId,
            recipeRevisionId,
            recipeTitle,
            text,
            comment,
            createdAt,
            updatedAt);

    /// <summary>
    /// Обновляет позицию меню.
    /// </summary>
    public Result Update(
        DateOnly date,
        Guid mealSlotId,
        RecipeId? recipeId,
        RecipeRevisionId? recipeRevisionId,
        string? recipeTitle,
        string? text,
        MenuServings servings,
        DateTimeOffset now,
        string? comment = null)
    {
        Result placement = ValidatePlacement(mealSlotId, Position);
        if (placement.IsFailure)
        {
            return Result.Failure(placement.Error);
        }

        if (recipeId.HasValue != recipeRevisionId.HasValue)
        {
            return Result.Failure(MenuCalendarErrors.InvalidItemPayload);
        }

        if (!recipeId.HasValue && string.IsNullOrWhiteSpace(text))
        {
            return Result.Failure(MenuCalendarErrors.EmptyTextItem);
        }

        Date = date;
        MealSlotId = mealSlotId;
        RecipeId = recipeId;
        RecipeRevisionId = recipeRevisionId;
        RecipeTitle = string.IsNullOrWhiteSpace(recipeTitle) ? null : recipeTitle.Trim();
        Text = string.IsNullOrWhiteSpace(text) ? null : text.Trim();
        Servings = servings;
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        UpdatedAt = now;
        return Result.Success();
    }

    /// <summary>
    /// Меняет порядок внутри приема пищи.
    /// </summary>
    public Result ChangePosition(int position, DateTimeOffset now)
    {
        if (position < 0)
        {
            return Result.Failure(MenuCalendarErrors.InvalidPosition);
        }

        Position = position;
        UpdatedAt = now;
        return Result.Success();
    }

    private static Result ValidatePlacement(Guid mealSlotId, int position)
    {
        if (mealSlotId == Guid.Empty)
        {
            return Result.Failure(MenuCalendarErrors.EmptyMealSlotId);
        }

        return position < 0
            ? Result.Failure(MenuCalendarErrors.InvalidPosition)
            : Result.Success();
    }
}
