using MenuMate.Api;
using MenuMate.Common.Infrastructure;
using MenuMate.Common.Presentation;
using MenuMate.Modules.Auth.Application;
using MenuMate.Modules.Auth.Infrastructure;
using MenuMate.Modules.Auth.Presentation;
using MenuMate.Modules.RecipeImports.Application;
using MenuMate.Modules.RecipeImports.Infrastructure;
using MenuMate.Modules.RecipeImports.Presentation;
using MenuMate.Modules.MenuPlanning.Application;
using MenuMate.Modules.MenuPlanning.Infrastructure;
using MenuMate.Modules.MenuPlanning.Presentation;
using MenuMate.Modules.Products.Infrastructure;
using MenuMate.Modules.Products.Presentation;
using MenuMate.Modules.Recipes.Application;
using MenuMate.Modules.Recipes.Infrastructure;
using MenuMate.Modules.Recipes.Presentation;
using MenuMate.Modules.ShoppingLists.Application;
using MenuMate.Modules.ShoppingLists.Infrastructure;
using MenuMate.Modules.ShoppingLists.Presentation;
using MenuMate.Modules.Tags.Application;
using MenuMate.Modules.Tags.Infrastructure;
using MenuMate.Modules.Tags.Presentation;
using MenuMate.ServiceDefaults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

string? dataProtectionKeysPath = builder.Configuration["MENUMATE_DATA_PROTECTION_KEYS_PATH"];
if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
}

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddOpenApi(options =>
{
    const string bearerSchemeName = "Bearer";

    options.AddDocumentTransformer((document, _, _) =>
    {
        OpenApiComponents components = document.Components ??= new OpenApiComponents();
        components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>(StringComparer.Ordinal);
        components.SecuritySchemes[bearerSchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT access token из auth endpoints."
        };

        return Task.CompletedTask;
    });

    options.AddOperationTransformer((operation, context, _) =>
    {
        IList<object> endpointMetadata = context.Description.ActionDescriptor.EndpointMetadata;
        bool requiresAuthorization = endpointMetadata.OfType<IAuthorizeData>().Any();
        bool allowsAnonymous = endpointMetadata.OfType<IAllowAnonymous>().Any();

        if (!requiresAuthorization || allowsAnonymous)
        {
            return Task.CompletedTask;
        }

        operation.Security ??= [];
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(bearerSchemeName, context.Document, null)] = []
        });

        operation.Responses ??= [];
        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
        operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

        return Task.CompletedTask;
    });
});
builder.Services.AddProblemDetailsResponses();
builder.Services.AddProblemDetailsAuthorization();
builder.Services
    .AddCommonInfrastructure(builder.Configuration)
    .AddAuthApplication()
    .AddAuthInfrastructure(builder.Configuration)
    .AddRecipeImportsApplication()
    .AddRecipeImportsInfrastructure(builder.Configuration)
    .AddRecipesApplication()
    .AddRecipesInfrastructure(builder.Configuration)
    .AddProductsInfrastructure(builder.Configuration)
    .AddTagsApplication()
    .AddTagsInfrastructure(builder.Configuration)
    .AddMenuPlanningApplication()
    .AddMenuPlanningInfrastructure(builder.Configuration)
    .AddShoppingListsApplication()
    .AddShoppingListsInfrastructure(builder.Configuration);

WebApplication app = builder.Build();

app.UseExceptionHandler();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("Diagnostics:ExposeApiDocs"))
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/", () => Results.Redirect("/api/system"))
    .WithName("Root");

app.MapGet("/health", (TimeProvider timeProvider) =>
        Results.Ok(new HealthResponse("Healthy", timeProvider.GetUtcNow())))
    .WithName("Health");

app.MapGet("/api/system", () => new SystemResponse(
        "MenuMate",
        "0.1.0",
        "Модульный монолит для рецептов, меню и списков покупок."))
    .WithName("GetSystemInfo");

app.MapGet("/api/modules", () => new[]
    {
        new ModuleResponse("Auth", "JWT auth, refresh tokens, users and roles."),
        new ModuleResponse("Recipes", "Рецепты, ингредиенты, шаги, избранное и связь с тегами."),
        new ModuleResponse("Products", "Общий нормализованный каталог продуктов и категории покупок."),
        new ModuleResponse("Tags", "Гибкие системные, пользовательские и предложенные теги."),
        new ModuleResponse("MenuPlanning", "Календарь питания, приемы пищи и запланированные блюда."),
        new ModuleResponse("ShoppingLists", "Расчет, группировка, отметки и текстовый шаринг покупок."),
        new ModuleResponse("RecipeImports", "Черновики импорта рецептов из изображений.")
    })
    .WithName("GetModules");

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapRecipeImportsEndpoints();
app.MapRecipesEndpoints();
app.MapProductsEndpoints();
app.MapTagsEndpoints();
app.MapMenuPlanningEndpoints();
app.MapShoppingListsEndpoints();

await app.RunAsync();

internal partial class Program
{
    private Program()
    {
    }
}
