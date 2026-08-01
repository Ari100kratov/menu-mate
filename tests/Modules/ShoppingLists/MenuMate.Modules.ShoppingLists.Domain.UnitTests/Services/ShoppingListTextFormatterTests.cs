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
        Assert.Contains("- Молоко 750 мл (для кофе)", text, StringComparison.Ordinal);
    }

    [Fact]
    public void FormatShouldPreservePurchasedStateInFullList()
    {
        var list = ShoppingList.FromItems(
            [
                new ShoppingListItem(
                    Guid.CreateVersion7(),
                    "Сыр",
                    "СЫР",
                    300m,
                    ShoppingUnit.Gram,
                    ShoppingProductCategory.Dairy,
                    null,
                    true)
            ]);

        string text = ShoppingListTextFormatter.Format(list);

        Assert.Contains("- ✓ Сыр 300 г", text, StringComparison.Ordinal);
    }

    [Fact]
    public void FormatRemainingShouldExcludePurchasedItemsAndEmptyCategories()
    {
        var list = ShoppingList.FromItems(
            [
                new ShoppingListItem(
                    Guid.CreateVersion7(),
                    "Сыр",
                    "СЫР",
                    300m,
                    ShoppingUnit.Gram,
                    ShoppingProductCategory.Dairy,
                    null,
                    true),
                new ShoppingListItem(
                    Guid.CreateVersion7(),
                    "Яблоки",
                    "ЯБЛОКИ",
                    1m,
                    ShoppingUnit.Kilogram,
                    ShoppingProductCategory.Produce,
                    null)
            ]);

        string text = ShoppingListTextFormatter.Format(list, ShoppingListTextScope.Remaining);

        Assert.DoesNotContain("Молочные продукты", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Сыр", text, StringComparison.Ordinal);
        Assert.Contains("Овощи и фрукты", text, StringComparison.Ordinal);
        Assert.Contains("- Яблоки 1 кг", text, StringComparison.Ordinal);
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
