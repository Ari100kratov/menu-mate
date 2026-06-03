using MenuMate.Modules.ShoppingLists.Domain.Services;

namespace MenuMate.Modules.ShoppingLists.Domain.Models;

/// <summary>
/// Список покупок, сгруппированный по категориям.
/// </summary>
public sealed record ShoppingList
{
    private ShoppingList(IReadOnlyCollection<ShoppingListCategory> categories)
    {
        Categories = categories;
    }

    /// <summary>
    /// Категории списка покупок.
    /// </summary>
    public IReadOnlyCollection<ShoppingListCategory> Categories { get; }

    /// <summary>
    /// Создает список покупок из плоского набора позиций.
    /// </summary>
    public static ShoppingList FromItems(IEnumerable<ShoppingListItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        ShoppingListCategory[] categories =
        [
            .. items
                .GroupBy(item => item.Category)
                .OrderBy(group => group.Key)
                .Select(group => new ShoppingListCategory(
                    group.Key,
                    ShoppingProductCategoryNames.GetDisplayName(group.Key),
                    [.. group.OrderBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase)]))
        ];

        return new ShoppingList(categories);
    }
}
