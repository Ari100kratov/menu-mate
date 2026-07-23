using MenuMate.Common.Application.Storage;
using MenuMate.Modules.Auth.Infrastructure.Database;
using MenuMate.Modules.RecipeImports.Infrastructure.Database;
using MenuMate.Modules.MenuPlanning.Infrastructure.Database;
using MenuMate.Modules.Products.Infrastructure.Database;
using MenuMate.Modules.Recipes.Infrastructure.Database;
using MenuMate.Modules.ShoppingLists.Infrastructure.Database;
using MenuMate.Modules.Tags.Infrastructure.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace MenuMate.Api.IntegrationTests;

internal sealed class MenuMateApiFactory : IAsyncLifetime, IDisposable
{
    private const string TestJwtIssuer = "MenuMate.IntegrationTests";
    private const string TestJwtAudience = "MenuMate.IntegrationTests";
    private const string TestJwtSecret = "integration-tests-secret-at-least-32-bytes";
    private const string TestJwtExpirationInMinutes = "60";

    private readonly PostgreSqlContainer _postgres = CreatePostgresContainer();
    private readonly Dictionary<string, string?> _previousEnvironmentValues = [];
    internal InMemoryObjectStorageService ObjectStorage { get; } = new();
    private InnerFactory? _factory;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        ConfigureEnvironment(_postgres.GetConnectionString());

        _factory = new InnerFactory(ObjectStorage);

        using IServiceScope scope = _factory.Services.CreateScope();
        IServiceProvider services = scope.ServiceProvider;

        await MigrateAsync<AuthDbContext>(services);
        await MigrateAsync<RecipeImportsDbContext>(services);
        await MigrateAsync<ProductsDbContext>(services);
        await MigrateAsync<TagsDbContext>(services);
        await MigrateAsync<RecipesDbContext>(services);
        await MigrateAsync<MenuPlanningDbContext>(services);
        await MigrateAsync<ShoppingListsDbContext>(services);
    }

    public async Task DisposeAsync()
    {
        try
        {
            if (_factory is not null)
            {
                await _factory.DisposeAsync();
            }

            await _postgres.DisposeAsync();
        }
        finally
        {
            RestoreEnvironment();
        }
    }

    public void Dispose()
    {
        try
        {
            _factory?.Dispose();
        }
        finally
        {
            RestoreEnvironment();
        }
    }

    public HttpClient CreateClient() =>
        (_factory ?? throw new InvalidOperationException("API factory is not initialized.")).CreateClient();

    internal IServiceProvider Services =>
        (_factory ?? throw new InvalidOperationException("API factory is not initialized.")).Services;

    private static PostgreSqlContainer CreatePostgresContainer()
    {
        string dockerConfig = Path.Combine(Path.GetTempPath(), "menumate-docker-config");
        Directory.CreateDirectory(dockerConfig);
        Environment.SetEnvironmentVariable("DOCKER_CONFIG", dockerConfig);

        return new PostgreSqlBuilder("postgres:18")
            .WithDatabase("menumate_tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    private static async Task MigrateAsync<TContext>(IServiceProvider services)
        where TContext : DbContext
    {
        await services.GetRequiredService<TContext>().Database.MigrateAsync();
    }

    private void ConfigureEnvironment(string connectionString)
    {
        SetEnvironmentVariable("ConnectionStrings__Database", connectionString);
        SetEnvironmentVariable("Jwt__Issuer", TestJwtIssuer);
        SetEnvironmentVariable("Jwt__Audience", TestJwtAudience);
        SetEnvironmentVariable("Jwt__Secret", TestJwtSecret);
        SetEnvironmentVariable("Jwt__ExpirationInMinutes", TestJwtExpirationInMinutes);
    }

    private void SetEnvironmentVariable(string name, string value)
    {
        if (!_previousEnvironmentValues.ContainsKey(name))
        {
            _previousEnvironmentValues[name] = Environment.GetEnvironmentVariable(name);
        }

        Environment.SetEnvironmentVariable(name, value);
    }

    private void RestoreEnvironment()
    {
        foreach (KeyValuePair<string, string?> environmentValue in _previousEnvironmentValues)
        {
            Environment.SetEnvironmentVariable(environmentValue.Key, environmentValue.Value);
        }

        _previousEnvironmentValues.Clear();
    }

    private sealed class InnerFactory(InMemoryObjectStorageService objectStorage)
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.UseEnvironment("IntegrationTests");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IObjectStorageService>();
                services.AddSingleton<IObjectStorageService>(objectStorage);
            });
        }
    }
}
