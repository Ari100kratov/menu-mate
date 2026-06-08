using System.Net;
using System.Net.Http.Json;
using MenuMate.Contracts.MenuPlanning;
using MenuMate.Contracts.Recipes;

namespace MenuMate.Api.IntegrationTests;

public sealed class RecipeSharingWorkflowTests : IAsyncLifetime, IDisposable
{
    private readonly MenuMateApiFactory _factory = new();

    public Task InitializeAsync() => _factory.InitializeAsync();

    public Task DisposeAsync() => _factory.DisposeAsync();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task PublicRecipeCanBeSavedAndCopiedButNotEditedByAnotherUser()
    {
        using HttpClient ownerHttpClient = _factory.CreateClient();
        using HttpClient readerHttpClient = _factory.CreateClient();
        var owner = new ApiTestClient(ownerHttpClient);
        var reader = new ApiTestClient(readerHttpClient);
        await owner.RegisterAsync(TestEmail.Create("sharing-owner"));
        await reader.RegisterAsync(TestEmail.Create("sharing-reader"));

        RecipeResponse original = await CreateRecipeAsync(ownerHttpClient, "Shared pasta", "Public");

        RecipeResponse? visible = await readerHttpClient.GetFromJsonAsync<RecipeResponse>(
            $"/api/recipes/{original.Id}");
        Assert.NotNull(visible);
        Assert.False(visible.IsOwnedByCurrentUser);
        Assert.False(visible.IsSaved);

        HttpResponseMessage saveResponse = await readerHttpClient.PostAsync(
            RelativeUri($"/api/recipes/{original.Id}/library"),
            content: null);
        saveResponse.EnsureSuccessStatusCode();

        RecipeListItemResponse[]? library = await readerHttpClient.GetFromJsonAsync<RecipeListItemResponse[]>(
            "/api/recipes?scope=library");
        Assert.NotNull(library);
        RecipeListItemResponse saved = Assert.Single(library);
        Assert.Equal(original.Id, saved.Id);
        Assert.True(saved.IsSaved);

        HttpResponseMessage copyResponse = await readerHttpClient.PostAsync(
            RelativeUri($"/api/recipes/{original.Id}/copy"),
            content: null);
        copyResponse.EnsureSuccessStatusCode();
        RecipeResponse? copy = await copyResponse.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(copy);
        Assert.NotEqual(original.Id, copy.Id);
        Assert.Equal(original.Id, copy.SourceRecipeId);
        Assert.Equal(original.CurrentRevisionId, copy.SourceRevisionId);
        Assert.Equal(original.Title, copy.Title);
        Assert.Equal("Private", copy.Visibility);
        Assert.True(copy.IsOwnedByCurrentUser);

        HttpResponseMessage editOriginalResponse = await readerHttpClient.PutAsJsonAsync(
            $"/api/recipes/{original.Id}",
            CreateRequest("Changed by reader", "Public"));
        await ProblemDetailsAssert.HasProblemAsync(
            editOriginalResponse,
            HttpStatusCode.Forbidden,
            "Recipes.AccessDenied");
    }

    [Fact]
    public async Task PrivateRecipeCannotBeReadByAnotherUser()
    {
        using HttpClient ownerHttpClient = _factory.CreateClient();
        using HttpClient readerHttpClient = _factory.CreateClient();
        var owner = new ApiTestClient(ownerHttpClient);
        var reader = new ApiTestClient(readerHttpClient);
        await owner.RegisterAsync(TestEmail.Create("private-owner"));
        await reader.RegisterAsync(TestEmail.Create("private-reader"));

        RecipeResponse recipe = await CreateRecipeAsync(ownerHttpClient, "Private soup", "Private");
        HttpResponseMessage response = await readerHttpClient.GetAsync(
            RelativeUri($"/api/recipes/{recipe.Id}"));

        await ProblemDetailsAssert.HasProblemAsync(response, HttpStatusCode.NotFound, "Recipes.NotFound");
    }

    [Fact]
    public async Task MenuRejectsPrivateForeignRecipeAndMismatchedRevision()
    {
        using HttpClient ownerHttpClient = _factory.CreateClient();
        using HttpClient readerHttpClient = _factory.CreateClient();
        var owner = new ApiTestClient(ownerHttpClient);
        var reader = new ApiTestClient(readerHttpClient);
        await owner.RegisterAsync(TestEmail.Create("menu-recipe-owner"));
        await reader.RegisterAsync(TestEmail.Create("menu-recipe-reader"));

        RecipeResponse first = await CreateRecipeAsync(ownerHttpClient, "First private recipe", "Private");
        RecipeResponse second = await CreateRecipeAsync(ownerHttpClient, "Second private recipe", "Private");
        MenuPlanResponse readerMenuPlan = await CreateMenuPlanAsync(readerHttpClient, "Reader week");
        MenuPlanResponse ownerMenuPlan = await CreateMenuPlanAsync(ownerHttpClient, "Owner week");

        HttpResponseMessage privateRecipeResponse = await AddRecipeToMenuAsync(
            readerHttpClient,
            readerMenuPlan.Id,
            first.Id,
            first.CurrentRevisionId);
        await ProblemDetailsAssert.HasProblemAsync(
            privateRecipeResponse,
            HttpStatusCode.Forbidden,
            "MenuPlanning.AccessDenied");

        HttpResponseMessage mismatchedRevisionResponse = await AddRecipeToMenuAsync(
            ownerHttpClient,
            ownerMenuPlan.Id,
            first.Id,
            second.CurrentRevisionId);
        await ProblemDetailsAssert.HasProblemAsync(
            mismatchedRevisionResponse,
            HttpStatusCode.Forbidden,
            "MenuPlanning.AccessDenied");
    }

    private static async Task<RecipeResponse> CreateRecipeAsync(
        HttpClient client,
        string title,
        string visibility)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/recipes/",
            CreateRequest(title, visibility));
        response.EnsureSuccessStatusCode();

        RecipeResponse? recipe = await response.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(recipe);
        return recipe;
    }

    private static CreateRecipeRequest CreateRequest(string title, string visibility) =>
        new(
            title,
            "Recipe sharing test",
            2,
            "MainCourse",
            visibility,
            30,
            15,
            null,
            [
                new RecipeIngredientRequest(
                    null,
                    "Pasta",
                    200m,
                    "Gram",
                    "Exact",
                    "GrainsAndPasta",
                    null,
                    false)
            ],
            [new PreparationStepRequest("Cook")],
            []);

    private static async Task<MenuPlanResponse> CreateMenuPlanAsync(HttpClient client, string name)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/menu-plans/",
            new CreateMenuPlanRequest(
                name,
                new DateOnly(2026, 6, 1),
                new DateOnly(2026, 6, 7)));
        response.EnsureSuccessStatusCode();

        MenuPlanResponse? menuPlan = await response.Content.ReadFromJsonAsync<MenuPlanResponse>();
        Assert.NotNull(menuPlan);
        return menuPlan;
    }

    private static Task<HttpResponseMessage> AddRecipeToMenuAsync(
        HttpClient client,
        Guid menuPlanId,
        Guid recipeId,
        Guid recipeRevisionId) =>
        client.PostAsJsonAsync(
            $"/api/menu-plans/{menuPlanId}/items",
            new CreateMenuPlanItemRequest(
                new DateOnly(2026, 6, 1),
                "Dinner",
                recipeId,
                recipeRevisionId,
                "Recipe",
                null,
                2,
                null));

    private static Uri RelativeUri(string path) => new(path, UriKind.Relative);
}
