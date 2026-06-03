using MenuMate.Modules.Recipes.Application.Abstractions;
using MenuMate.Modules.Recipes.Application.UploadRecipeImage;
using MenuMate.Modules.Recipes.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MenuMate.Modules.Recipes.Infrastructure;

/// <summary>
/// Регистрация инфраструктуры модуля Recipes.
/// </summary>
public static class RecipesInfrastructureDependencyInjection
{
    /// <summary>
    /// Добавляет PostgreSQL persistence модуля Recipes.
    /// </summary>
    public static IServiceCollection AddRecipesInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        string connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' is not configured.");

        services.AddDbContext<RecipesDbContext>(options =>
        {
            options.UseNpgsql(
                    connectionString,
                    npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                        HistoryRepository.DefaultTableName,
                        RecipesSchema.Name))
                .UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IRecipesRepository, EfRecipesRepository>();
        services.AddScoped<IRecipeImagesRepository, EfRecipeImagesRepository>();
        services.AddScoped<IRecipesUnitOfWork>(provider => provider.GetRequiredService<RecipesDbContext>());
        services.AddScoped<IRecipesReadDbContext>(provider => provider.GetRequiredService<RecipesDbContext>());
        services.AddSingleton(CreateRecipeImageStorageOptions(configuration));

        return services;
    }

    private static RecipeImageStorageOptions CreateRecipeImageStorageOptions(IConfiguration configuration)
    {
        IConfigurationSection minioSection = configuration.GetSection("Minio");
        string bucketName = minioSection["ImagesBucketName"] ?? "images";

        return new RecipeImageStorageOptions
        {
            BucketName = bucketName
        };
    }
}
