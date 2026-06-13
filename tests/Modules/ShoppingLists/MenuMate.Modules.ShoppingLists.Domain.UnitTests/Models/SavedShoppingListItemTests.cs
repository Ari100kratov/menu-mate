using MenuMate.Modules.ShoppingLists.Domain.Enums;
using MenuMate.Modules.ShoppingLists.Domain.Errors;
using MenuMate.Modules.ShoppingLists.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.ShoppingLists.Domain.UnitTests.Models;

public sealed class SavedShoppingListItemTests
{
    [Fact]
    public void CreateShouldTrimAndNormalizeValues()
    {
        SavedShoppingListItem item = SavedShoppingListItem.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "  Рис басмати  ",
            "",
            500m,
            ShoppingUnit.Gram,
            ShoppingProductCategory.GrainsAndPasta,
            "  промыть  ").Value;

        Assert.Equal("Рис басмати", item.Name);
        Assert.Equal("РИС БАСМАТИ", item.NormalizedName);
        Assert.Equal("промыть", item.Comment);
        Assert.False(item.IsPurchased);
    }

    [Fact]
    public void CreateShouldAllowToTasteWithoutAmount()
    {
        SavedShoppingListItem item = SavedShoppingListItem.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Соль",
            "СОЛЬ",
            null,
            ShoppingUnit.ToTaste,
            ShoppingProductCategory.Spices,
            null).Value;

        Assert.Null(item.Amount);
        Assert.Equal(ShoppingUnit.ToTaste, item.Unit);
    }

    [Theory]
    [InlineData("", 1, ShoppingUnit.Piece, "empty")]
    [InlineData("Рис", null, ShoppingUnit.Gram, "required")]
    [InlineData("Рис", 0, ShoppingUnit.Gram, "positive")]
    public void CreateShouldRejectInvalidItem(
        string name,
        int? amount,
        ShoppingUnit unit,
        string expectedError)
    {
        Result<SavedShoppingListItem> result = SavedShoppingListItem.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            name,
            "",
            amount,
            unit,
            ShoppingProductCategory.Other,
            null);

        Assert.True(result.IsFailure);
        Assert.Equal(
            expectedError switch
            {
                "empty" => ShoppingListErrors.EmptyItemName,
                "required" => ShoppingListErrors.AmountRequired,
                _ => ShoppingListErrors.AmountMustBePositive
            },
            result.Error);
    }

    [Fact]
    public void WithStateShouldReturnUpdatedCopy()
    {
        SavedShoppingListItem item = CreateItem();

        SavedShoppingListItem purchased = item.WithState(isPurchased: true);

        Assert.False(item.IsPurchased);
        Assert.True(purchased.IsPurchased);
        Assert.Equal(item.Id, purchased.Id);
    }

    private static SavedShoppingListItem CreateItem() =>
        SavedShoppingListItem.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Рис",
            "РИС",
            1m,
            ShoppingUnit.Piece,
            ShoppingProductCategory.GrainsAndPasta,
            null).Value;
}
