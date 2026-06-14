#pragma warning disable OPENAI001 // Responses API помечен experimental в официальном SDK OpenAI 2.11.0.

using MenuMate.Modules.RecipeImports.Application;
using MenuMate.Modules.RecipeImports.Application.Abstractions;
using MenuMate.Modules.RecipeImports.Application.Extraction;
using MenuMate.Modules.RecipeImports.Application.Generation;
using MenuMate.Modules.RecipeImports.Infrastructure.Database;
using MenuMate.Modules.RecipeImports.Infrastructure.Cleanup;
using MenuMate.Modules.RecipeImports.Infrastructure.OpenAI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Responses;
using OpenAI.Images;

namespace MenuMate.Modules.RecipeImports.Infrastructure;

/// <summary>
/// Регистрация инфраструктуры модуля Imports.
/// </summary>
public static class RecipeImportsInfrastructureDependencyInjection
{
    /// <summary>Добавляет persistence и OpenAI-интеграцию модуля Imports.</summary>
    public static IServiceCollection AddRecipeImportsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        string connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' is not configured.");

        services.AddDbContext<RecipeImportsDbContext>(options =>
        {
            options.UseNpgsql(
                    connectionString,
                    npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                        HistoryRepository.DefaultTableName,
                        RecipeImportsSchema.Name))
                .UseSnakeCaseNamingConvention();
        });

        OpenAiRecipeImageExtractorOptions openAiOptions = CreateOpenAiOptions(configuration);
        services.AddSingleton(openAiOptions);
        services.AddSingleton(new ResponsesClient(openAiOptions.ApiKey));
        OpenAiRecipeCoverImageGeneratorOptions imageOptions = CreateImageOptions(configuration);
        services.AddSingleton(imageOptions);
        services.AddSingleton(new ImageClient(imageOptions.Model, imageOptions.ApiKey));
        services.AddSingleton(CreateStorageOptions(configuration));
        services.AddHostedService<RecipeImportDraftCleanupService>();
        services.AddScoped<IRecipeImageExtractor, OpenAiRecipeImageExtractor>();
        services.AddScoped<IRecipeCoverImageGenerator, OpenAiRecipeCoverImageGenerator>();
        services.AddScoped<IRecipeImportDraftRepository, EfRecipeImportDraftRepository>();
        services.AddScoped<IRecipeImportsUnitOfWork>(
            provider => provider.GetRequiredService<RecipeImportsDbContext>());
        return services;
    }

    private static OpenAiRecipeImageExtractorOptions CreateOpenAiOptions(IConfiguration configuration) =>
        new()
        {
            ApiKey = ResolveApiKey(configuration),
            Model = configuration["OpenAI:Model"] ?? "gpt-5.4-mini"
        };

    private static OpenAiRecipeCoverImageGeneratorOptions CreateImageOptions(IConfiguration configuration) =>
        new()
        {
            ApiKey = ResolveApiKey(configuration),
            Model = configuration["OpenAI:ImageModel"] ?? "gpt-image-1-mini"
        };

    private static string ResolveApiKey(IConfiguration configuration)
    {
        string? configuredApiKey = configuration["OpenAI:ApiKey"];
        if (!string.IsNullOrWhiteSpace(configuredApiKey))
        {
            return configuredApiKey;
        }

        string? environmentApiKey = configuration["OPENAI_API_KEY"];
        return string.IsNullOrWhiteSpace(environmentApiKey) ? "not-configured" : environmentApiKey;
    }

    private static RecipeImportStorageOptions CreateStorageOptions(IConfiguration configuration) =>
        new()
        {
            BucketName = configuration["Minio:ImportsBucketName"] ?? "imports",
            DraftRetentionPeriod = TimeSpan.FromDays(
                GetPositiveInt(configuration["RecipeImports:DraftRetentionDays"], 7)),
            CleanupInterval = TimeSpan.FromMinutes(
                GetPositiveInt(configuration["RecipeImports:CleanupIntervalMinutes"], 60))
        };

    private static int GetPositiveInt(string? value, int defaultValue) =>
        int.TryParse(value, out int parsedValue) && parsedValue > 0 ? parsedValue : defaultValue;
}
