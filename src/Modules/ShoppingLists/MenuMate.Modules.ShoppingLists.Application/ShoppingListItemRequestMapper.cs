using MenuMate.Contracts.ShoppingLists;
using MenuMate.Common.Application.Products;
using MenuMate.Modules.ShoppingLists.Domain.Enums;
using MenuMate.Modules.ShoppingLists.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.ShoppingLists.Application;

internal static class ShoppingListItemRequestMapper
{
    public static Result<SavedShoppingListItem> Map(
        Guid itemId,
        ShoppingListItemRequest request,
        ProductCatalogItem product)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!Enum.TryParse(request.Unit, ignoreCase: true, out ShoppingUnit unit))
        {
            return Result.Failure<SavedShoppingListItem>(ShoppingListApplicationErrors.InvalidUnit);
        }

        if (!Enum.TryParse(product.Category, ignoreCase: true, out ShoppingProductCategory category))
        {
            return Result.Failure<SavedShoppingListItem>(ShoppingListApplicationErrors.InvalidProductCategory);
        }

        return SavedShoppingListItem.Create(
            itemId,
            product.Id,
            product.Name,
            TextNormalizer.NormalizeSearchText(product.Name),
            request.Amount,
            unit,
            category,
            request.Comment);
    }
}
