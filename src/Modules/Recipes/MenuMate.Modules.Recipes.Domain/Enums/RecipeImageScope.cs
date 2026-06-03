namespace MenuMate.Modules.Recipes.Domain.Enums;

/// <summary>
/// Область рецепта, к которой привязано изображение.
/// </summary>
public enum RecipeImageScope
{
    /// <summary>
    /// Область не указана.
    /// </summary>
    None = 0,

    /// <summary>
    /// Обложка рецепта.
    /// </summary>
    Cover = 1,

    /// <summary>
    /// Изображение конкретного шага приготовления.
    /// </summary>
    Step = 2
}
