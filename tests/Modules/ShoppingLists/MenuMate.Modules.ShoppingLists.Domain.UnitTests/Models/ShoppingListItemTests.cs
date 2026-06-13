using MenuMate.Modules.ShoppingLists.Domain.Enums;
using MenuMate.Modules.ShoppingLists.Domain.Models;

namespace MenuMate.Modules.ShoppingLists.Domain.UnitTests.Models;

public sealed class ShoppingListItemTests
{
    [Theory]
    [InlineData(ShoppingUnit.Gram, true)]
    [InlineData(ShoppingUnit.Milliliter, true)]
    [InlineData(ShoppingUnit.Piece, true)]
    [InlineData(ShoppingUnit.Pack, false)]
    [InlineData(ShoppingUnit.ToTaste, false)]
    public void CanMergeShouldDependOnUnit(ShoppingUnit unit, bool expected)
    {
        decimal? amount = unit == ShoppingUnit.ToTaste ? null : 1m;
        var item = new ShoppingListItem(
            Guid.CreateVersion7(),
            "Продукт",
            "ПРОДУКТ",
            amount,
            unit,
            ShoppingProductCategory.Other,
            null);

        Assert.Equal(expected, item.CanMerge);
    }

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
