namespace MenuMate.Modules.ShoppingLists.Domain.Enums;

/// <summary>
/// Тип количества позиции списка покупок.
/// </summary>
public enum ShoppingQuantityKind
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

