using MenuMate.Modules.ShoppingLists.Domain.Enums;

namespace MenuMate.Modules.ShoppingLists.Domain.ValueObjects;

/// <summary>
/// Ингредиент рецепта в виде входной строки для генерации списка покупок.
/// </summary>
/// <param name="ProductId">Идентификатор продукта общего каталога.</param>
/// <param name="Name">Отображаемое название продукта.</param>
/// <param name="NormalizedName">Нормализованное название продукта.</param>
/// <param name="Amount">Количество, если оно числовое.</param>
/// <param name="Unit">Единица измерения.</param>
/// <param name="Category">Категория продукта.</param>
/// <param name="Comment">Комментарий.</param>
/// <param name="IsOptional">Признак необязательного ингредиента.</param>
/// <param name="LineId">Идентификатор строки ингредиента в ревизии рецепта.</param>
public sealed record ShoppingIngredientLine(
    Guid ProductId,
    string Name,
    string NormalizedName,
    decimal? Amount,
    ShoppingUnit Unit,
    ShoppingProductCategory Category,
    string? Comment,
    bool IsOptional,
    Guid LineId = default);
