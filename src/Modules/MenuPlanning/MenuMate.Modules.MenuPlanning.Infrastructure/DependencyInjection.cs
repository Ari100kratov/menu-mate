using MenuMate.Modules.MenuPlanning.Application.Abstractions;
using MenuMate.Modules.MenuPlanning.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MenuMate.Modules.MenuPlanning.Infrastructure;

/// <summary>
/// Регистрация зависимостей инфраструктуры планирования меню.
/// </summary>
public static class MenuPlanningInfrastructureDependencyInjection
{
    /// <summary>
    /// Добавляет PostgreSQL persistence модуля MenuPlanning.
    /// </summary>
    public static IServiceCollection AddMenuPlanningInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' is not configured.");

        services.AddDbContext<MenuPlanningDbContext>(options =>
        {
            options.UseNpgsql(
                    connectionString,
                    npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                        HistoryRepository.DefaultTableName,
                        MenuPlanningSchema.Name))
                .UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IMenuPlansRepository, EfMenuPlansRepository>();
        services.AddScoped<IMenuPlansUnitOfWork>(provider => provider.GetRequiredService<MenuPlanningDbContext>());
        services.AddScoped<IMenuPlansReadDbContext>(provider => provider.GetRequiredService<MenuPlanningDbContext>());
        services.AddScoped<IRecipeRevisionAccessReader, RecipeRevisionAccessReader>();

        return services;
    }
}
