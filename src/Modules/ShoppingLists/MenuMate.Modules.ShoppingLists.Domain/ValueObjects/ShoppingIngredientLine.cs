using MenuMate.Modules.ShoppingLists.Domain.Enums;

namespace MenuMate.Modules.ShoppingLists.Domain.ValueObjects;

/// <summary>
/// Ингредиент рецепта в виде входной строки для генерации списка покупок.
/// </summary>
/// <param name="Name">Отображаемое название продукта.</param>
/// <param name="NormalizedName">Нормализованное название продукта.</param>
/// <param name="Amount">Количество, если оно числовое.</param>
/// <param name="Unit">Единица измерения.</param>
/// <param name="QuantityKind">Тип количества.</param>
/// <param name="Category">Категория продукта.</param>
/// <param name="Comment">Комментарий.</param>
/// <param name="IsOptional">Признак необязательного ингредиента.</param>
public sealed record ShoppingIngredientLine(
    string Name,
    string NormalizedName,
    decimal? Amount,
    ShoppingUnit Unit,
    ShoppingQuantityKind QuantityKind,
    ShoppingProductCategory Category,
    string? Comment,
    bool IsOptional);

