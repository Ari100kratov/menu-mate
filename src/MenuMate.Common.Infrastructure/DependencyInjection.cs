using MenuMate.Common.Application.Storage;
using MenuMate.Common.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace MenuMate.Common.Infrastructure;

/// <summary>
/// Регистрация общей инфраструктуры.
/// </summary>
public static class CommonInfrastructureDependencyInjection
{
    /// <summary>
    /// Добавляет общие инфраструктурные сервисы.
    /// </summary>
    public static IServiceCollection AddCommonInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services.AddObjectStorage(configuration);

    private static IServiceCollection AddObjectStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(CreateMinioOptions(configuration));

        services.AddSingleton<IMinioClient>(serviceProvider =>
        {
            MinioOptions options = serviceProvider.GetRequiredService<MinioOptions>();

            return new MinioClient()
                .WithEndpoint(options.Endpoint)
                .WithCredentials(options.AccessKey, options.SecretKey)
                .WithSSL(options.UseSsl)
                .Build();
        });

        services.AddSingleton<IObjectStorageService, MinioObjectStorageService>();

        return services;
    }

    private static MinioOptions CreateMinioOptions(IConfiguration configuration)
    {
        IConfigurationSection section = configuration.GetSection("Minio");

        return new MinioOptions
        {
            Endpoint = section["Endpoint"] ?? string.Empty,
            AccessKey = section["AccessKey"] ?? string.Empty,
            SecretKey = section["SecretKey"] ?? string.Empty,
            ImagesBucketName = section["ImagesBucketName"] ?? "images",
            UseSsl = bool.TryParse(section["UseSsl"], out bool useSsl) && useSsl
        };
    }
}
