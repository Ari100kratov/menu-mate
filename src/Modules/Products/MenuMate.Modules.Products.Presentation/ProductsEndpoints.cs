using MenuMate.Common.Application.Products;
using MenuMate.Contracts.Products;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MenuMate.Modules.Products.Presentation;

/// <summary>
/// HTTP-конечные точки общего каталога продуктов.
/// </summary>
public static class ProductsEndpoints
{
    /// <summary>
    /// Регистрирует конечные точки общего каталога продуктов.
    /// </summary>
    public static IEndpointRouteBuilder MapProductsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products", GetProductsAsync)
            .WithTags("Products")
            .WithName("GetProducts")
            .RequireAuthorization()
            .Produces<IReadOnlyCollection<ProductResponse>>();

        return app;
    }

    private static async Task<IResult> GetProductsAsync(
        string? search,
        IProductCatalog catalog,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<ProductCatalogItem> products = await catalog.SearchAsync(search, cancellationToken);
        return Results.Ok(products.Select(product => new ProductResponse(
            product.Id,
            product.Name,
            product.Category)));
    }
}
