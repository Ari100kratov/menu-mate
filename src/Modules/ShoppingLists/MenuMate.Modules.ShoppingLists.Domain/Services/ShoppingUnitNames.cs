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
            ShoppingUnit.Glass => "стакан",
            ShoppingUnit.Cup => "чашка",
            ShoppingUnit.Dessertspoon => "дес. л.",
            ShoppingUnit.Clove => "зубчик",
            ShoppingUnit.Bunch => "пучок",
            ShoppingUnit.Sprig => "веточка",
            ShoppingUnit.Head => "головка",
            ShoppingUnit.Stalk => "стебель",
            ShoppingUnit.Slice => "ломтик",
            ShoppingUnit.Sheet => "лист",
            ShoppingUnit.Handful => "горсть",
            ShoppingUnit.Drop => "капля",
            ShoppingUnit.Can => "банка",
            ShoppingUnit.Jar => "банка",
            ShoppingUnit.Bottle => "бутылка",
            ShoppingUnit.Sachet => "пакетик",
            ShoppingUnit.Cube => "кубик",
            _ => string.Empty
        };
}
