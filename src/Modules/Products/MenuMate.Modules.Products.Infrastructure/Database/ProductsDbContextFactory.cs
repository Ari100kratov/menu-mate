using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MenuMate.Modules.Products.Infrastructure.Database;

internal sealed class ProductsDbContextFactory : IDesignTimeDbContextFactory<ProductsDbContext>
{
    public ProductsDbContext CreateDbContext(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var optionsBuilder = new DbContextOptionsBuilder<ProductsDbContext>();
        optionsBuilder
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=menumate;Username=postgres",
                options => options.MigrationsHistoryTable(HistoryRepository.DefaultTableName, ProductsSchema.Name))
            .UseSnakeCaseNamingConvention();

        return new ProductsDbContext(optionsBuilder.Options);
    }
}
