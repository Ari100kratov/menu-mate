using MenuMate.Modules.ShoppingLists.Domain.Enums;

namespace MenuMate.Modules.ShoppingLists.Domain.ValueObjects;

/// <summary>
/// Ручная позиция списка покупок.
/// </summary>
/// <param name="Name">Название продукта.</param>
/// <param name="NormalizedName">Нормализованное название.</param>
/// <param name="Amount">Количество.</param>
/// <param name="Unit">Единица измерения.</param>
/// <param name="QuantityKind">Тип количества.</param>
/// <param name="Category">Категория.</param>
/// <param name="Comment">Комментарий.</param>
public sealed record ManualShoppingListLine(
    string Name,
    string NormalizedName,
    decimal? Amount,
    ShoppingUnit Unit,
    ShoppingQuantityKind QuantityKind,
    ShoppingProductCategory Category,
    string? Comment);
