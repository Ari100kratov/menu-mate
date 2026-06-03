using MenuMate.Contracts.ShoppingLists;
using MenuMate.Modules.ShoppingLists.Domain.Enums;
using MenuMate.Modules.ShoppingLists.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.ShoppingLists.Application;

internal static class ShoppingListItemRequestMapper
{
    public static Result<SavedShoppingListItem> Map(Guid itemId, ShoppingListItemRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!Enum.TryParse(request.Unit, ignoreCase: true, out ShoppingUnit unit))
        {
            return Result.Failure<SavedShoppingListItem>(ShoppingListApplicationErrors.InvalidUnit);
        }

        if (!Enum.TryParse(request.QuantityKind, ignoreCase: true, out ShoppingQuantityKind quantityKind))
        {
            return Result.Failure<SavedShoppingListItem>(ShoppingListApplicationErrors.InvalidQuantityKind);
        }

        if (!Enum.TryParse(request.Category, ignoreCase: true, out ShoppingProductCategory category))
        {
            return Result.Failure<SavedShoppingListItem>(ShoppingListApplicationErrors.InvalidProductCategory);
        }

        return SavedShoppingListItem.Create(
            itemId,
            request.Name,
            TextNormalizer.NormalizeSearchText(request.Name),
            request.Amount,
            unit,
            quantityKind,
            category,
            request.Comment);
    }
}
