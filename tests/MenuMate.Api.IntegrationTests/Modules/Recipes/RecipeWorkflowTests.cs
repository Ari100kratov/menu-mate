using System.Net;
using System.Net.Http.Json;
using MenuMate.Contracts.Recipes;

namespace MenuMate.Api.IntegrationTests;

public sealed class RecipeWorkflowTests : IAsyncLifetime, IDisposable
{
    private readonly MenuMateApiFactory _factory = new();

    public Task InitializeAsync() => _factory.InitializeAsync();

    public Task DisposeAsync() => _factory.DisposeAsync();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task OwnedRecipeShouldSupportCreateUpdateFavoriteFilterAndDelete()
    {
        using HttpClient httpClient = _factory.CreateClient();
        var client = new ApiTestClient(httpClient);
        await client.RegisterAsync(TestEmail.Create("recipe-workflow"));

        RecipeResponse created = await CreateRecipeAsync(httpClient, CreateRequest("Паста", "Private"));
        Assert.Equal(1, created.RevisionNumber);
        Assert.True(created.IsOwnedByCurrentUser);
        Assert.Equal("Паста", created.Title);
        Assert.Equal("GrainsAndPasta", Assert.Single(created.Ingredients).Category);

        HttpResponseMessage updateResponse = await httpClient.PutAsJsonAsync(
            $"/api/recipes/{created.Id}",
            CreateRequest("Паста с овощами", "Public"));
        updateResponse.EnsureSuccessStatusCode();

        RecipeResponse? updated = await httpClient.GetFromJsonAsync<RecipeResponse>($"/api/recipes/{created.Id}");
        Assert.NotNull(updated);
        Assert.Equal(2, updated.RevisionNumber);
        Assert.NotEqual(created.CurrentRevisionId, updated.CurrentRevisionId);
        Assert.Equal("Паста с овощами", updated.Title);
        Assert.Equal("Public", updated.Visibility);

        HttpResponseMessage favoriteResponse = await httpClient.PostAsync(
            RelativeUri($"/api/recipes/{created.Id}/favorite"),
            content: null);
        favoriteResponse.EnsureSuccessStatusCode();

        RecipeListItemResponse[]? favorites = await httpClient.GetFromJsonAsync<RecipeListItemResponse[]>(
            "/api/recipes?favoritesOnly=true");
        Assert.NotNull(favorites);
        Assert.Equal(created.Id, Assert.Single(favorites).Id);

        HttpResponseMessage deleteResponse = await httpClient.DeleteAsync(
            RelativeUri($"/api/recipes/{created.Id}"));
        deleteResponse.EnsureSuccessStatusCode();

        HttpResponseMessage getDeletedResponse = await httpClient.GetAsync(
            RelativeUri($"/api/recipes/{created.Id}"));
        await ProblemDetailsAssert.HasProblemAsync(
            getDeletedResponse,
            HttpStatusCode.NotFound,
            "Recipes.NotFound");
    }

    [Fact]
    public async Task RecipeListShouldFilterBySearchAndTag()
    {
        using HttpClient httpClient = _factory.CreateClient();
        var client = new ApiTestClient(httpClient);
        await client.RegisterAsync(TestEmail.Create("recipe-filters"));

        await CreateRecipeAsync(httpClient, CreateRequest("Быстрая паста", "Private", ["ужин", "быстро"]));
        await CreateRecipeAsync(httpClient, CreateRequest("Овсянка", "Private", ["завтрак"]));

        RecipeListItemResponse[]? search = await httpClient.GetFromJsonAsync<RecipeListItemResponse[]>(
            "/api/recipes?search=паста");
        RecipeListItemResponse[]? tag = await httpClient.GetFromJsonAsync<RecipeListItemResponse[]>(
            "/api/recipes?tag=БЫСТРО");

        Assert.NotNull(search);
        Assert.NotNull(tag);
        Assert.Equal("Быстрая паста", Assert.Single(search).Title);
        Assert.Equal("Быстрая паста", Assert.Single(tag).Title);
    }

    private static async Task<RecipeResponse> CreateRecipeAsync(HttpClient client, CreateRecipeRequest request)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/recipes/", request);
        response.EnsureSuccessStatusCode();
        RecipeResponse? recipe = await response.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(recipe);
        return recipe;
    }

    private static CreateRecipeRequest CreateRequest(
        string title,
        string visibility,
        IReadOnlyCollection<string>? tags = null) =>
        new(
            title,
            "Описание рецепта",
            2,
            "MainCourse",
            visibility,
            30,
            15,
            new Uri("https://example.com/recipe"),
            [
                new RecipeIngredientRequest(
                    null,
                    "Паста",
                    200m,
                    "Gram",
                    "GrainsAndPasta",
                    "отварить",
                    false)
            ],
            [new PreparationStepRequest("Приготовить")],
            tags ?? []);

    private static Uri RelativeUri(string path) => new(path, UriKind.Relative);
}
