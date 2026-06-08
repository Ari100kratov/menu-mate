namespace MenuMate.Common.Application.Products;

/// <summary>
/// Общий каталог продуктов для рецептов и списков покупок.
/// </summary>
public interface IProductCatalog
{
    /// <summary>
    /// Возвращает существующий продукт или создаёт новый по нормализованному названию.
    /// </summary>
    Task<ProductCatalogItem> ResolveAsync(
        Guid? productId,
        string name,
        string category,
        CancellationToken cancellationToken);

    /// <summary>
    /// Ищет продукты каталога для автодополнения.
    /// </summary>
    Task<IReadOnlyCollection<ProductCatalogItem>> SearchAsync(
        string? search,
        CancellationToken cancellationToken);
}

/// <summary>
/// Продукт из общего каталога.
/// </summary>
public sealed record ProductCatalogItem(Guid Id, string Name, string Category);
