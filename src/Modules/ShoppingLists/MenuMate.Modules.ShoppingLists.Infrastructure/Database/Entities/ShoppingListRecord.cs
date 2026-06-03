using MenuMate.Modules.ShoppingLists.Domain.Models;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.ShoppingLists.Infrastructure.Database.Entities;

internal sealed class ShoppingListRecord
{
    public Guid Id { get; set; }

    public UserId OwnerUserId { get; set; }

    public MenuPlanId SourceMenuPlanId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public List<ShoppingListItemRecord> Items { get; set; } = [];

    public static ShoppingListRecord FromDomain(SavedShoppingList shoppingList)
    {
        var record = new ShoppingListRecord();
        record.Apply(shoppingList);
        return record;
    }

    public void Apply(SavedShoppingList shoppingList)
    {
        Id = shoppingList.Id;
        OwnerUserId = shoppingList.OwnerUserId;
        SourceMenuPlanId = shoppingList.SourceMenuPlanId;
        CreatedAt = shoppingList.CreatedAt;
        UpdatedAt = shoppingList.UpdatedAt;

        Items.Clear();
        Items.AddRange(shoppingList.Items.Select(ShoppingListItemRecord.FromDomain));

        foreach (ShoppingListItemRecord item in Items)
        {
            item.ShoppingListId = Id;
        }
    }

    public SavedShoppingList ToDomain() =>
        SavedShoppingList.Rehydrate(
            Id,
            OwnerUserId,
            SourceMenuPlanId,
            CreatedAt,
            UpdatedAt,
            Items.Select(item => item.ToDomain()));
}
