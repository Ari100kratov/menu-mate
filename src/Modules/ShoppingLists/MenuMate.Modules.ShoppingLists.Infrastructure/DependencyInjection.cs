using MenuMate.Modules.ShoppingLists.Application.Abstractions;
using MenuMate.Modules.ShoppingLists.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MenuMate.Modules.ShoppingLists.Infrastructure;

/// <summary>
/// Регистрация инфраструктуры модуля списков покупок.
/// </summary>
public static class ShoppingListsInfrastructureDependencyInjection
{
    /// <summary>
    /// Добавляет PostgreSQL persistence модуля ShoppingLists.
    /// </summary>
    public static IServiceCollection AddShoppingListsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' is not configured.");

        services.AddDbContext<ShoppingListsDbContext>(options =>
        {
            options.UseNpgsql(
                    connectionString,
                    npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                        HistoryRepository.DefaultTableName,
                        ShoppingListsSchema.Name))
                .UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IShoppingListsRepository, EfShoppingListsRepository>();
        services.AddScoped<IShoppingListsUnitOfWork>(provider => provider.GetRequiredService<ShoppingListsDbContext>());
        services.AddScoped<IShoppingListsReadDbContext>(provider => provider.GetRequiredService<ShoppingListsDbContext>());
        services.AddScoped<IShoppingListSourceReader>(provider => provider.GetRequiredService<ShoppingListsDbContext>());

        return services;
    }
}
