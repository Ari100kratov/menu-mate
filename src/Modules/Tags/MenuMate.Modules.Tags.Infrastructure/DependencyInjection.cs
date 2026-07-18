using MenuMate.Modules.Tags.Application.Abstractions;
using MenuMate.Common.Application.Tags;
using MenuMate.Modules.Tags.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MenuMate.Modules.Tags.Infrastructure;

/// <summary>
/// Регистрация зависимостей инфраструктуры тегов.
/// </summary>
public static class TagsInfrastructureDependencyInjection
{
    /// <summary>
    /// Добавляет PostgreSQL persistence модуля Tags.
    /// </summary>
    public static IServiceCollection AddTagsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' is not configured.");

        services.AddDbContext<TagsDbContext>(options =>
        {
            options.UseNpgsql(
                    connectionString,
                    npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                        HistoryRepository.DefaultTableName,
                        TagsSchema.Name))
                .UseSnakeCaseNamingConvention();
        });

        services.AddScoped<ITagsRepository, EfTagsRepository>();
        services.AddScoped<ITagsUnitOfWork>(provider => provider.GetRequiredService<TagsDbContext>());
        services.AddScoped<ITagsReadDbContext>(provider => provider.GetRequiredService<TagsDbContext>());
        services.AddScoped<ITagCatalog, TagCatalog>();

        return services;
    }
}
