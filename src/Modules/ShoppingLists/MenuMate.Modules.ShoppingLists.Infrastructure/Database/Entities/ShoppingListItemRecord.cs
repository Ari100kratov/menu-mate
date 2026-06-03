using MenuMate.Modules.ShoppingLists.Domain.Enums;
using MenuMate.Modules.ShoppingLists.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.ShoppingLists.Infrastructure.Database.Entities;

internal sealed class ShoppingListItemRecord
{
    public Guid Id { get; set; }

    public Guid ShoppingListId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string NormalizedName { get; set; } = string.Empty;

    public decimal? Amount { get; set; }

    public ShoppingUnit Unit { get; set; }

    public ShoppingQuantityKind QuantityKind { get; set; }

    public ShoppingProductCategory Category { get; set; }

    public string? Comment { get; set; }

    public bool IsPurchased { get; set; }

    public bool IsInStock { get; set; }

    public static ShoppingListItemRecord FromDomain(SavedShoppingListItem item) =>
        new()
        {
            Id = item.Id,
            Name = item.Name,
            NormalizedName = item.NormalizedName,
            Amount = item.Amount,
            Unit = item.Unit,
            QuantityKind = item.QuantityKind,
            Category = item.Category,
            Comment = item.Comment,
            IsPurchased = item.IsPurchased,
            IsInStock = item.IsInStock
        };

    public SavedShoppingListItem ToDomain()
    {
        Result<SavedShoppingListItem> item = SavedShoppingListItem.Create(
            Id,
            Name,
            NormalizedName,
            Amount,
            Unit,
            QuantityKind,
            Category,
            Comment,
            IsPurchased,
            IsInStock);

        if (item.IsFailure)
        {
            throw new DomainException(item.Error);
        }

        return item.Value;
    }
}
