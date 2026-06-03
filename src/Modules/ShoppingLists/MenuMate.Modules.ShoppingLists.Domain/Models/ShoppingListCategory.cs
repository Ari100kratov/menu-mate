using MenuMate.Modules.ShoppingLists.Domain.Enums;

namespace MenuMate.Modules.ShoppingLists.Domain.Models;

/// <summary>
/// Группа позиций списка покупок по категории.
/// </summary>
/// <param name="Category">Категория.</param>
/// <param name="Name">Отображаемое название категории.</param>
/// <param name="Items">Позиции категории.</param>
public sealed record ShoppingListCategory(
    ShoppingProductCategory Category,
    string Name,
    IReadOnlyCollection<ShoppingListItem> Items);

