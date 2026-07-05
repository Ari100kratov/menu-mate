#pragma warning disable OPENAI001 // Responses API остается экспериментальным в официальном SDK OpenAI 2.11.0.

using MenuMate.Common.Application;
using MenuMate.Common.Infrastructure;
using MenuMate.DataImporter;
using MenuMate.DataImporter.Infrastructure.Database;
using MenuMate.DataImporter.Recipes;
using MenuMate.DataImporter.Wikibooks;
using MenuMate.Modules.Auth.Infrastructure;
using MenuMate.Modules.Auth.Infrastructure.Database;
using MenuMate.Modules.Products.Infrastructure;
using MenuMate.Modules.Products.Infrastructure.Database;
using MenuMate.Modules.Recipes.Application;
using MenuMate.Modules.Recipes.Infrastructure;
using MenuMate.Modules.Recipes.Infrastructure.Database;
using MenuMate.ServiceDefaults;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using OpenAI.Responses;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

var importOptions = ImportOptions.Create(builder.Configuration, args);
var importerUserContext = new ImporterUserContext();
string? configuredConnectionString = importOptions.ConnectionString ??
    builder.Configuration.GetConnectionString("Database");
if (string.IsNullOrWhiteSpace(configuredConnectionString))
{
    throw new InvalidOperationException(
        "Строка подключения 'Database' не настроена. Передайте --connection-string \"...\" " +
        "или задайте ConnectionStrings__Database. При использовании Aspire скопируйте строку " +
        "подключения ресурса базы данных из Aspire Dashboard.");
}

string connectionString = EnsureDatabaseIsSpecified(configuredConnectionString);
builder.Configuration["ConnectionStrings:Database"] = connectionString;
string openAiApiKey = builder.Configuration["OpenAI:ApiKey"] ??
    builder.Configuration["OPENAI_API_KEY"] ??
    "not-configured";

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(importOptions);
builder.Services.AddSingleton(importerUserContext);
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
    .AddCommonInfrastructure(builder.Configuration)
    .AddAuthInfrastructure(builder.Configuration)
    .AddProductsInfrastructure(builder.Configuration)
    .AddRecipesApplication()
    .AddRecipesInfrastructure(builder.Configuration);
builder.Services.AddSingleton<IUserContext>(importerUserContext);

using IHost host = builder.Build();
await using AsyncServiceScope scope = host.Services.CreateAsyncScope();
IServiceProvider services = scope.ServiceProvider;

if (!importOptions.DryRun)
{
    await services.GetRequiredService<DataImportDbContext>().Database.MigrateAsync();
}

if (string.IsNullOrWhiteSpace(importOptions.AdminEmail))
{
    throw new InvalidOperationException(
        "Укажите email существующего администратора через --admin-email или DataImport:AdminEmail.");
}
UserId? adminUserId = await services.GetRequiredService<AuthDbContext>()
    .FindAdminUserIdByEmailAsync(importOptions.AdminEmail, CancellationToken.None)
    ?? throw new InvalidOperationException(
        $"Пользователь '{importOptions.AdminEmail}' с ролью admin не найден.");

importerUserContext.SetUser(adminUserId.Value);
using var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(importOptions.UserAgent);
var wikibooksClient = new WikibooksClient(httpClient, importOptions);
var openAiExtractor = new OpenAiRecipeTextExtractor(new ResponsesClient(openAiApiKey), builder.Configuration);
var runner = new ImportRunner(
    importOptions,
    services.GetRequiredService<DataImportDbContext>(),
    wikibooksClient,
    openAiExtractor,
    services.GetRequiredService<ICommandHandler<
        MenuMate.Modules.Recipes.Application.CreateRecipe.CreateRecipeCommand,
        MenuMate.Contracts.Recipes.RecipeResponse>>(),
    services.GetRequiredService<ICommandHandler<
        MenuMate.Modules.Recipes.Application.UploadRecipeImage.UploadRecipeImageCommand,
        MenuMate.Contracts.Recipes.RecipeImageResponse>>(),
    services.GetRequiredService<TimeProvider>(),
    services.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ImportRunner>>());
await runner.RunAsync(CancellationToken.None);

static string EnsureDatabaseIsSpecified(string connectionString)
{
    var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
    if (string.IsNullOrWhiteSpace(connectionStringBuilder.Database))
    {
        throw new InvalidOperationException(
            "В строке подключения не указана база данных. Добавьте Database=menumate. " +
            "Не используйте строку подключения серверного ресурса postgres из Aspire Dashboard.");
    }

    return connectionStringBuilder.ConnectionString;
}
