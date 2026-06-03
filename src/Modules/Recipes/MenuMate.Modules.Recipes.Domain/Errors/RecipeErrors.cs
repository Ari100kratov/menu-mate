using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Domain.Errors;

/// <summary>
/// Ошибки домена рецептов.
/// </summary>
public static class RecipeErrors
{
    /// <summary>
    /// Название рецепта пустое.
    /// </summary>
    public static readonly AppError EmptyTitle = AppError.Validation(
        "Recipes.EmptyTitle",
        "Название рецепта не может быть пустым.");

    /// <summary>
    /// Название рецепта слишком длинное.
    /// </summary>
    public static readonly AppError TitleTooLong = AppError.Validation(
        "Recipes.TitleTooLong",
        "Название рецепта не должно быть длиннее 160 символов.");

    /// <summary>
    /// Количество персон некорректно.
    /// </summary>
    public static readonly AppError InvalidServings = AppError.Validation(
        "Recipes.InvalidServings",
        "Количество персон должно быть от 1 до 100.");

    /// <summary>
    /// Название продукта пустое.
    /// </summary>
    public static readonly AppError EmptyIngredientName = AppError.Validation(
        "Recipes.EmptyIngredientName",
        "Название продукта не может быть пустым.");

    /// <summary>
    /// Количество ингредиента некорректно.
    /// </summary>
    public static readonly AppError InvalidIngredientAmount = AppError.Validation(
        "Recipes.InvalidIngredientAmount",
        "Количество ингредиента должно быть больше нуля.");

    /// <summary>
    /// Текст шага приготовления пустой.
    /// </summary>
    public static readonly AppError EmptyStepText = AppError.Validation(
        "Recipes.EmptyStepText",
        "Шаг приготовления не может быть пустым.");
}


