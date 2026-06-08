using Microsoft.EntityFrameworkCore;

namespace MenuMate.Modules.Products.Infrastructure.Database;

/// <summary>
/// EF Core DbContext общего каталога продуктов.
/// </summary>
public sealed class ProductsDbContext(DbContextOptions<ProductsDbContext> options) : DbContext(options)
{
    internal DbSet<ProductRecord> Products => Set<ProductRecord>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(ProductsSchema.Name);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductsDbContext).Assembly);
    }
}
