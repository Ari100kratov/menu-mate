using MenuMate.Modules.ShoppingLists.Domain.Enums;
using MenuMate.Modules.ShoppingLists.Domain.Models;

namespace MenuMate.Modules.ShoppingLists.Domain.UnitTests.Models;

public sealed class ShoppingListItemTests
{
    [Fact]
    public void MarkPurchasedShouldReturnUpdatedCopy()
    {
        var item = new ShoppingListItem(
            Guid.CreateVersion7(),
            "Продукт",
            "ПРОДУКТ",
            1m,
            ShoppingUnit.Piece,
            ShoppingProductCategory.Other,
            null);

        ShoppingListItem purchased = item.MarkPurchased();

        Assert.False(item.IsPurchased);
        Assert.True(purchased.IsPurchased);
    }
}
