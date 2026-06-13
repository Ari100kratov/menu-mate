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
        Guid readerDinnerSlotId = await GetDinnerSlotIdAsync(readerHttpClient);
        Guid ownerDinnerSlotId = await GetDinnerSlotIdAsync(ownerHttpClient);

        HttpResponseMessage privateRecipeResponse = await AddRecipeToMenuAsync(
            readerHttpClient,
            readerDinnerSlotId,
            first.Id,
            first.CurrentRevisionId);
        await ProblemDetailsAssert.HasProblemAsync(
            privateRecipeResponse,
            HttpStatusCode.Forbidden,
            "MenuPlanning.AccessDenied");

        HttpResponseMessage mismatchedRevisionResponse = await AddRecipeToMenuAsync(
            ownerHttpClient,
            ownerDinnerSlotId,
            first.Id,
            second.CurrentRevisionId);
        await ProblemDetailsAssert.HasProblemAsync(
            mismatchedRevisionResponse,
            HttpStatusCode.Forbidden,
            "MenuPlanning.AccessDenied");
    }

    [Fact]
    public async Task CatalogAllowsSameProductNameInDifferentCategories()
    {
        using HttpClient httpClient = _factory.CreateClient();
        var user = new ApiTestClient(httpClient);
        await user.RegisterAsync(TestEmail.Create("product-category-variants"));

        RecipeResponse first = await CreateRecipeAsync(
            httpClient,
            CreateRequestWithIngredient("Chicken as other", "Chicken", "Other"));
        RecipeResponse second = await CreateRecipeAsync(
            httpClient,
            CreateRequestWithIngredient("Chicken as meat", "Chicken", "MeatAndPoultry"));

        IngredientResponse firstIngredient = Assert.Single(first.Ingredients);
        IngredientResponse secondIngredient = Assert.Single(second.Ingredients);
        Assert.NotEqual(firstIngredient.IngredientId, secondIngredient.IngredientId);
        Assert.Equal("Other", firstIngredient.Category);
        Assert.Equal("MeatAndPoultry", secondIngredient.Category);
    }

    private static async Task<RecipeResponse> CreateRecipeAsync(
        HttpClient client,
        string title,
        string visibility)
    {
        return await CreateRecipeAsync(client, CreateRequest(title, visibility));
    }

    private static async Task<RecipeResponse> CreateRecipeAsync(
        HttpClient client,
        CreateRecipeRequest request)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/recipes/", request);
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
                    "GrainsAndPasta",
                    null,
                    false)
            ],
            [new PreparationStepRequest("Cook")],
            []);

    private static CreateRecipeRequest CreateRequestWithIngredient(
        string title,
        string productName,
        string category) =>
        new(
            title,
            "Product category variant test",
            2,
            "MainCourse",
            "Private",
            30,
            15,
            null,
            [
                new RecipeIngredientRequest(
                    null,
                    productName,
                    200m,
                    "Gram",
                    category,
                    null,
                    false)
            ],
            [new PreparationStepRequest("Cook")],
            []);

    private static async Task<Guid> GetDinnerSlotIdAsync(HttpClient client)
    {
        MenuCalendarResponse? calendar = await client.GetFromJsonAsync<MenuCalendarResponse>(
            "/api/menu-calendar?startDate=2026-06-01&endDate=2026-06-07");
        Assert.NotNull(calendar);
        return Assert.Single(calendar.MealSlots, slot => slot.Name == "Ужин").Id;
    }

    private static Task<HttpResponseMessage> AddRecipeToMenuAsync(
        HttpClient client,
        Guid mealSlotId,
        Guid recipeId,
        Guid recipeRevisionId) =>
        client.PostAsJsonAsync(
            "/api/menu-calendar/items",
            new CreateMenuCalendarItemRequest(
                new DateOnly(2026, 6, 1),
                mealSlotId,
                recipeId,
                recipeRevisionId,
                "Recipe",
                null,
                2,
                null));

    private static Uri RelativeUri(string path) => new(path, UriKind.Relative);
}
