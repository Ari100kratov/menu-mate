using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.Modules.Recipes.Domain.ValueObjects;

namespace MenuMate.Modules.Recipes.Domain.Models;

/// <summary>
/// Ингредиент рецепта.
/// </summary>
/// <param name="IngredientId">Идентификатор продукта общего каталога.</param>
/// <param name="Name">Название продукта.</param>
/// <param name="Quantity">Количество.</param>
/// <param name="Category">Категория для списка покупок.</param>
/// <param name="Comment">Комментарий к ингредиенту.</param>
/// <param name="IsOptional">Признак необязательного ингредиента.</param>
public sealed record RecipeIngredient(
    Guid? IngredientId,
    IngredientName Name,
    IngredientQuantity Quantity,
    ProductCategory Category,
    string? Comment,
    bool IsOptional)
{
    /// <summary>
    /// Возвращает новый ингредиент с масштабированным количеством.
    /// </summary>
    public RecipeIngredient Scale(decimal factor) => this with { Quantity = Quantity.Scale(factor) };
}
