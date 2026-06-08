using MenuMate.Common.Application.Products;
using MenuMate.Contracts.ShoppingLists;
using MenuMate.Modules.ShoppingLists.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.ShoppingLists.Application;

internal sealed class ShoppingProductResolver(IProductCatalog productCatalog)
{
    public async Task<Result<SavedShoppingListItem>> ResolveAsync(
        Guid itemId,
        ShoppingListItemRequest request,
        CancellationToken cancellationToken)
    {
        ProductCatalogItem product = await productCatalog.ResolveAsync(
            request.ProductId,
            request.Name,
            request.Category,
            cancellationToken);

        return ShoppingListItemRequestMapper.Map(itemId, request, product);
    }
}
