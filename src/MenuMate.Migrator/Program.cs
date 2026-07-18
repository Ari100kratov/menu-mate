using MenuMate.Migrator;
using MenuMate.DataImporter.Infrastructure.Database;
using MenuMate.Modules.Auth.Infrastructure;
using MenuMate.Modules.Auth.Infrastructure.Database;
using MenuMate.Modules.RecipeImports.Infrastructure;
using MenuMate.Modules.RecipeImports.Infrastructure.Database;
using MenuMate.Modules.MenuPlanning.Infrastructure;
using MenuMate.Modules.MenuPlanning.Infrastructure.Database;
using MenuMate.Modules.Products.Infrastructure;
using MenuMate.Modules.Products.Infrastructure.Database;
using MenuMate.Modules.Recipes.Infrastructure;
using MenuMate.Modules.Recipes.Infrastructure.Database;
using MenuMate.Modules.ShoppingLists.Infrastructure;
using MenuMate.Modules.ShoppingLists.Infrastructure.Database;
using MenuMate.Modules.Tags.Infrastructure;
using MenuMate.Modules.Tags.Infrastructure.Database;
using MenuMate.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSingleton(TimeProvider.System);
string? configuredConnectionString = builder.Configuration.GetConnectionString("Database");
if (string.IsNullOrWhiteSpace(configuredConnectionString))
{
    throw new InvalidOperationException(
        "Connection string 'Database' is not configured. " +
        "Set ConnectionStrings__Database before starting MenuMate.Migrator outside Aspire.");
}

string connectionString = configuredConnectionString;
builder.Services.AddDbContext<DataImportDbContext>(options =>
{
    options.UseNpgsql(
            connectionString,
            npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                HistoryRepository.DefaultTableName,
                DataImportSchema.Name))
        .UseSnakeCaseNamingConvention();
});
builder.Services
    .AddAuthInfrastructure(builder.Configuration)
    .AddRecipeImportsInfrastructure(builder.Configuration)
    .AddRecipesInfrastructure(builder.Configuration)
    .AddProductsInfrastructure(builder.Configuration)
    .AddTagsInfrastructure(builder.Configuration)
    .AddMenuPlanningInfrastructure(builder.Configuration)
    .AddShoppingListsInfrastructure(builder.Configuration);

using IHost host = builder.Build();

await using AsyncServiceScope scope = host.Services.CreateAsyncScope();

IServiceProvider services = scope.ServiceProvider;
ILogger logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("MenuMate.Migrator");

MigratorLogMessages.StartingMigrations(logger);

await MigrateDbContextAsync<RecipeImportsDbContext>(services, logger);
await MigrateDbContextAsync<ProductsDbContext>(services, logger);
await MigrateDbContextAsync<AuthDbContext>(services, logger);
await MigrateDbContextAsync<TagsDbContext>(services, logger);
await MigrateDbContextAsync<RecipesDbContext>(services, logger);
await MigrateDbContextAsync<MenuPlanningDbContext>(services, logger);
await MigrateDbContextAsync<ShoppingListsDbContext>(services, logger);
await MigrateDbContextAsync<DataImportDbContext>(services, logger);

MigratorLogMessages.MigrationsCompleted(logger);

static async Task MigrateDbContextAsync<TContext>(IServiceProvider services, ILogger logger)
    where TContext : DbContext
{
    string dbContextName = typeof(TContext).Name;

    MigratorLogMessages.StartingDbContextMigration(logger, dbContextName);

    await services.GetRequiredService<TContext>().Database.MigrateAsync();

    MigratorLogMessages.DbContextMigrationCompleted(logger, dbContextName);
}
