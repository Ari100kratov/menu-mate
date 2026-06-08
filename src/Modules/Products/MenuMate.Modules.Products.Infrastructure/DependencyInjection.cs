using MenuMate.Common.Application.Products;
using MenuMate.Modules.Products.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MenuMate.Modules.Products.Infrastructure;

/// <summary>
/// Регистрация инфраструктуры общего каталога продуктов.
/// </summary>
public static class ProductsInfrastructureDependencyInjection
{
    /// <summary>
    /// Добавляет persistence и сервис общего каталога продуктов.
    /// </summary>
    public static IServiceCollection AddProductsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' is not configured.");

        services.AddDbContext<ProductsDbContext>(options =>
        {
            options.UseNpgsql(
                    connectionString,
                    npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                        HistoryRepository.DefaultTableName,
                        ProductsSchema.Name))
                .UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IProductCatalog, ProductCatalog>();

        return services;
    }
}
