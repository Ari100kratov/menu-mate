using MenuMate.Modules.ShoppingLists.Domain.ValueObjects;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.ShoppingLists.Domain.Models;

/// <summary>
/// Снимок рецепта для расчета списка покупок.
/// </summary>
/// <param name="RecipeId">Идентификатор рецепта.</param>
/// <param name="BaseServings">Количество персон в исходном рецепте.</param>
/// <param name="TargetServings">Количество персон в меню.</param>
/// <param name="Ingredients">Ингредиенты рецепта.</param>
/// <param name="MenuItemId">Идентификатор блюда в календаре меню.</param>
/// <param name="Title">Название блюда.</param>
public sealed record ShoppingRecipe(
    RecipeId RecipeId,
    int BaseServings,
    int TargetServings,
    IReadOnlyCollection<ShoppingIngredientLine> Ingredients,
    Guid MenuItemId = default,
    string Title = "Рецепт");
