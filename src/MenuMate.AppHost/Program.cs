using Microsoft.Extensions.Configuration;

// Some VPN clients block Aspire DCP's default IPv6 loopback endpoint.
Environment.SetEnvironmentVariable("DCP_IP_VERSION_PREFERENCE", "IPv4");

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> postgres = builder
    .AddPostgres("postgres")
    .WithImage("postgres:18")
    .WithDataVolume();

IResourceBuilder<PostgresDatabaseResource> database = postgres.AddDatabase("menumate", "menumate");

IResourceBuilder<ContainerResource> minio = builder
    .AddContainer("minio", "minio/minio", "latest")
    .WithBindMount("../../.containers/minio", "/data")
    .WithEnvironment("MINIO_ROOT_USER", "admin")
    .WithEnvironment("MINIO_ROOT_PASSWORD", "password123")
    .WithEnvironment("MINIO_API_CORS_ALLOW_ORIGIN", "http://localhost,http://localhost:5173")
    .WithArgs("server", "/data", "--console-address", ":9001")
    .WithEndpoint(port: 9000, targetPort: 9000, name: "api")
    .WithEndpoint(port: 9001, targetPort: 9001, name: "console");

IResourceBuilder<ContainerResource> minioInit = builder
    .AddContainer("minio-init", "minio/mc", "latest")
    .WithEntrypoint("/bin/sh")
    .WithArgs(
        "-c",
        """
        set -e
        until mc alias set local http://minio:9000 admin password123; do sleep 2; done
        mc mb --ignore-existing "local/images"
        mc mb --ignore-existing "local/imports"
        mc anonymous set download "local/images"
        """)
    .WaitFor(minio);

IResourceBuilder<ProjectResource> migrator = builder.AddProject<Projects.MenuMate_Migrator>("migrator")
    .WithEnvironment("ConnectionStrings__Database", database)
    .WithReference(database)
    .WaitFor(database);

IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.MenuMate_Api>("api")
    .WithEnvironment("ConnectionStrings__Database", database)
    .WithEnvironment("Minio__Endpoint", "localhost:9000")
    .WithEnvironment("Minio__AccessKey", "admin")
    .WithEnvironment("Minio__SecretKey", "password123")
    .WithEnvironment("Minio__ImagesBucketName", "images")
    .WithEnvironment("Minio__ImportsBucketName", "imports")
    .WithEnvironment("Minio__UseSsl", "false")
    .WithEnvironment("OpenAI__ApiKey", ResolveOpenAiApiKey(builder.Configuration))
    .WithEnvironment("OpenAI__Model", builder.Configuration["OpenAI:Model"] ?? "gpt-5.4-mini")
    .WithEnvironment("OpenAI__ImageModel", builder.Configuration["OpenAI:ImageModel"] ?? "gpt-image-1-mini")
    .WithEnvironment("RecipeImports__DraftRetentionDays", builder.Configuration["RecipeImports:DraftRetentionDays"] ?? "7")
    .WithEnvironment("RecipeImports__CleanupIntervalMinutes", builder.Configuration["RecipeImports:CleanupIntervalMinutes"] ?? "60")
    .WithReference(database)
    .WaitFor(database)
    .WaitFor(minio)
    .WaitForCompletion(minioInit)
    .WaitForCompletion(migrator)
    .WithHttpHealthCheck("/health");

builder.AddViteApp("web", "../MenuMate.Web", "dev:host")
    .WithPnpm(install: false)
    .WithEnvironment("VITE_API_URL", string.Empty)
    .WithEnvironment("VITE_API_PROXY_TARGET", api.GetEndpoint("http"))
    .WithReference(api)
    .WaitFor(api);

await builder.Build().RunAsync();

static string ResolveOpenAiApiKey(IConfiguration configuration)
{
    string? configuredApiKey = configuration["OpenAI:ApiKey"];
    return !string.IsNullOrWhiteSpace(configuredApiKey)
        ? configuredApiKey
        : configuration["OPENAI_API_KEY"] ?? string.Empty;
}
