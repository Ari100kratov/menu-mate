using MenuMate.Modules.ShoppingLists.Domain.Enums;
using MenuMate.Modules.ShoppingLists.Domain.Models;
using MenuMate.Modules.ShoppingLists.Domain.Services;

namespace MenuMate.Modules.ShoppingLists.Domain.UnitTests.Services;

public sealed class ShoppingListTextFormatterTests
{
    [Fact]
    public void FormatShouldCreateGroupedCopyableTextWithComments()
    {
        var list = ShoppingList.FromItems(
            [
                new ShoppingListItem(
                    Guid.CreateVersion7(),
                    "Молоко",
                    "МОЛОКО",
                    750m,
                    ShoppingUnit.Milliliter,
                    ShoppingProductCategory.Dairy,
                    "для кофе")
            ]);

        string text = ShoppingListTextFormatter.Format(list);

        Assert.Contains("Молочные продукты", text, StringComparison.Ordinal);
        Assert.Contains("- [ ] Молоко 750 мл (для кофе)", text, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null, ShoppingUnit.ToTaste, "по вкусу")]
    [InlineData(2, ShoppingUnit.Unknown, "2")]
    [InlineData(null, ShoppingUnit.Unknown, "")]
    public void FormatAmountShouldHandleSpecialQuantities(
        int? amount,
        ShoppingUnit unit,
        string expected)
    {
        var item = new ShoppingListItem(
            Guid.CreateVersion7(),
            "Продукт",
            "ПРОДУКТ",
            amount,
            unit,
            ShoppingProductCategory.Other,
            null);

        Assert.Equal(expected, ShoppingListTextFormatter.FormatAmount(item));
    }
}
