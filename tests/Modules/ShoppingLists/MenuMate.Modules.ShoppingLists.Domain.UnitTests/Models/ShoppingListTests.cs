using MenuMate.Modules.ShoppingLists.Domain.Enums;
using MenuMate.Modules.ShoppingLists.Domain.Models;

namespace MenuMate.Modules.ShoppingLists.Domain.UnitTests.Models;

public sealed class ShoppingListTests
{
    [Fact]
    public void FromItemsShouldGroupByCategoryAndSortItemsByName()
    {
        var list = ShoppingList.FromItems(
            [
                CreateItem("Яблоко", ShoppingProductCategory.Produce),
                CreateItem("Абрикос", ShoppingProductCategory.Produce),
                CreateItem("Молоко", ShoppingProductCategory.Dairy)
            ]);

        Assert.Equal(2, list.Categories.Count);
        ShoppingListCategory produce = Assert.Single(
            list.Categories,
            category => category.Category == ShoppingProductCategory.Produce);
        Assert.Equal(["Абрикос", "Яблоко"], produce.Items.Select(item => item.Name));
    }

    [Fact]
    public void FromItemsShouldRejectNull()
    {
        Assert.Throws<ArgumentNullException>(() => ShoppingList.FromItems(null!));
    }

    private static ShoppingListItem CreateItem(string name, ShoppingProductCategory category) =>
        new(Guid.CreateVersion7(), name, name.ToUpperInvariant(), 1m, ShoppingUnit.Piece, category, null);
}
