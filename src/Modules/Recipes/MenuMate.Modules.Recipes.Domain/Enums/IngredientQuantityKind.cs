namespace MenuMate.Modules.Recipes.Domain.Enums;

/// <summary>
/// Тип количества ингредиента.
/// </summary>
public enum IngredientQuantityKind
{
    /// <summary>
    /// Точное количество.
    /// </summary>
    Exact = 0,

    /// <summary>
    /// Примерное количество.
    /// </summary>
    Approximate = 1,

    /// <summary>
    /// Количество по вкусу или желанию.
    /// </summary>
    ToTaste = 2
}

