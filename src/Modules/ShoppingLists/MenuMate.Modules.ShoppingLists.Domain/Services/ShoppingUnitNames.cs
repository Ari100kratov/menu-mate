using MenuMate.Modules.ShoppingLists.Domain.Enums;

namespace MenuMate.Modules.ShoppingLists.Domain.Services;

/// <summary>
/// Отображаемые названия единиц измерения.
/// </summary>
public static class ShoppingUnitNames
{
    /// <summary>
    /// Возвращает короткое русское название единицы.
    /// </summary>
    public static string GetDisplayName(ShoppingUnit unit) =>
        unit switch
        {
            ShoppingUnit.Gram => "г",
            ShoppingUnit.Kilogram => "кг",
            ShoppingUnit.Milliliter => "мл",
            ShoppingUnit.Liter => "л",
            ShoppingUnit.Piece => "шт",
            ShoppingUnit.Teaspoon => "ч. л.",
            ShoppingUnit.Tablespoon => "ст. л.",
            ShoppingUnit.Pinch => "щепотка",
            ShoppingUnit.Pack => "уп.",
            ShoppingUnit.ToTaste => "по вкусу",
            _ => string.Empty
        };
}

