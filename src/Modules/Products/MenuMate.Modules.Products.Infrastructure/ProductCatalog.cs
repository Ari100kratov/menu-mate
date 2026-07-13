using MenuMate.Common.Application.Products;
using MenuMate.Modules.Products.Infrastructure.Database;
using MenuMate.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace MenuMate.Modules.Products.Infrastructure;

/// <summary>
/// Persistence-реализация общего каталога продуктов.
/// </summary>
public sealed class ProductCatalog : IProductCatalog
{
    private readonly ProductsDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Инициализирует persistence-реализацию общего каталога продуктов.
    /// </summary>
    public ProductCatalog(ProductsDbContext dbContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public async Task<ProductCatalogItem> ResolveAsync(
        Guid? productId,
        string name,
        string category,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(category);

        ProductRecord? product = productId is { } id
            ? await _dbContext.Products.FirstOrDefaultAsync(item => item.Id == id, cancellationToken)
            : null;

        string normalizedName = ProductNameNormalizer.Normalize(name);
        string normalizedNameForComparison = ProductNameNormalizer.NormalizeForComparison(name);
        product ??= await _dbContext.Products.FirstOrDefaultAsync(
            item => item.Category == category &&
                (item.NormalizedName == normalizedNameForComparison ||
                 item.NormalizedName.Replace("Ё", "Е") == normalizedNameForComparison),
            cancellationToken);

        if (product is null)
        {
            product = new ProductRecord
            {
                Id = Guid.CreateVersion7(),
                Name = normalizedName,
                NormalizedName = normalizedNameForComparison,
                Category = category,
                CreatedAt = _timeProvider.GetUtcNow()
            };

            await _dbContext.Products.AddAsync(product, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return ToItem(product);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<ProductCatalogItem>> SearchAsync(
        string? search,
        CancellationToken cancellationToken)
    {
        IQueryable<ProductRecord> query = _dbContext.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            string normalized = ProductNameNormalizer.NormalizeForComparison(search);
            query = query.Where(product => product.NormalizedName
                .Replace("Ё", "Е")
                .Contains(normalized));
        }

        return await query
            .OrderBy(product => product.Name)
            .Take(30)
            .Select(product => new ProductCatalogItem(product.Id, product.Name, product.Category))
            .ToArrayAsync(cancellationToken);
    }

    private static ProductCatalogItem ToItem(ProductRecord product) =>
        new(product.Id, product.Name, product.Category);
}
