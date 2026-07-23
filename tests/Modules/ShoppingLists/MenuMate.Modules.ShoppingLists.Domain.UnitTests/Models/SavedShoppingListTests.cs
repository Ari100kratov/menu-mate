using MenuMate.Modules.ShoppingLists.Domain.Enums;
using MenuMate.Modules.ShoppingLists.Domain.Models;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.ShoppingLists.Domain.UnitTests.Models;

public sealed class SavedShoppingListTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 6, 12, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void AddUpdateStateAndRemoveShouldMaintainSingleList()
    {
        SavedShoppingList list = CreateList([]);
        SavedShoppingListItem item = CreateSavedItem("Рис", 500m);

        list.AddItem(item, FixedNow.AddMinutes(1));
        Assert.True(list.SetItemState(item.Id, isPurchased: true, FixedNow.AddMinutes(2)));

        SavedShoppingListItem updated = CreateSavedItem("Рис басмати", 1000m, item.Id);
        Assert.True(list.UpdateItem(item.Id, updated, FixedNow.AddMinutes(3)));
        Assert.Equal("Рис басмати", Assert.Single(list.Items).Name);

        Assert.True(list.RemoveItem(item.Id, FixedNow.AddMinutes(4)));
        Assert.Empty(list.Items);
        Assert.Equal(FixedNow.AddMinutes(4), list.UpdatedAt);
        Assert.Null(list.SourceStartDate);
        Assert.Null(list.SourceEndDate);
    }

    [Fact]
    public void MissingItemOperationsShouldReturnFalseWithoutChangingTimestamp()
    {
        SavedShoppingList list = CreateList([]);

        Assert.False(list.SetItemState(Guid.CreateVersion7(), true, FixedNow.AddMinutes(1)));
        Assert.False(list.UpdateItem(Guid.CreateVersion7(), CreateSavedItem("Рис", 1m), FixedNow.AddMinutes(1)));
        Assert.False(list.RemoveItem(Guid.CreateVersion7(), FixedNow.AddMinutes(1)));
        Assert.Equal(FixedNow, list.UpdatedAt);
    }

    [Fact]
    public void ReplaceShouldReplaceItemsRangeAndResetGeneratedState()
    {
        ShoppingListItem initial = CreateGeneratedItem("Рис", 500m) with { IsPurchased = true };
        SavedShoppingList list = CreateList([initial]);
        DateOnly start = new(2026, 6, 15);
        DateOnly end = new(2026, 6, 21);

        Result result = list.Replace(
            start,
            end,
            [CreateGeneratedItem("Молоко", 1000m)],
            FixedNow.AddDays(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(start, list.SourceStartDate);
        Assert.Equal(end, list.SourceEndDate);
        SavedShoppingListItem item = Assert.Single(list.Items);
        Assert.Equal("Молоко", item.Name);
        Assert.False(item.IsPurchased);
    }

    [Fact]
    public void CreateManualListShouldNotHaveMenuSourcePeriod()
    {
        SavedShoppingList list = SavedShoppingList.Create(
            Guid.CreateVersion7(),
            UserId.Create(),
            null,
            null,
            [],
            FixedNow).Value;

        list.AddItem(CreateSavedItem("Рис", 500m), FixedNow.AddMinutes(1));

        Assert.Null(list.SourceStartDate);
        Assert.Null(list.SourceEndDate);
    }

    private static SavedShoppingList CreateList(IEnumerable<ShoppingListItem> items) =>
        SavedShoppingList.Create(
            Guid.CreateVersion7(),
            UserId.Create(),
            new DateOnly(2026, 6, 8),
            new DateOnly(2026, 6, 14),
            items,
            FixedNow).Value;

    private static ShoppingListItem CreateGeneratedItem(string name, decimal amount) =>
        new(
            Guid.CreateVersion7(),
            name,
            name.ToUpperInvariant(),
            amount,
            ShoppingUnit.Gram,
            ShoppingProductCategory.GrainsAndPasta,
            null);

    private static SavedShoppingListItem CreateSavedItem(string name, decimal amount, Guid? id = null) =>
        SavedShoppingListItem.Create(
            id ?? Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            name,
            name.ToUpperInvariant(),
            amount,
            ShoppingUnit.Gram,
            ShoppingProductCategory.GrainsAndPasta,
            null).Value;
}
