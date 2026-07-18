using System.Net;
using System.Net.Http.Json;
using MenuMate.Contracts.Recipes;
using MenuMate.Contracts.Tags;

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

        HttpResponseMessage identicalUpdateResponse = await httpClient.PutAsJsonAsync(
            $"/api/recipes/{created.Id}",
            CreateRequest("Паста с овощами", "Public"));
        identicalUpdateResponse.EnsureSuccessStatusCode();
        RecipeResponse? afterIdenticalUpdate = await httpClient.GetFromJsonAsync<RecipeResponse>(
            $"/api/recipes/{created.Id}");
        Assert.NotNull(afterIdenticalUpdate);
        Assert.Equal(updated.RevisionId, afterIdenticalUpdate.RevisionId);
        Assert.Equal(2, afterIdenticalUpdate.RevisionNumber);

        HttpResponseMessage visibilityUpdateResponse = await httpClient.PutAsJsonAsync(
            $"/api/recipes/{created.Id}",
            CreateRequest("Паста с овощами", "Private"));
        visibilityUpdateResponse.EnsureSuccessStatusCode();
        RecipeResponse? afterVisibilityUpdate = await httpClient.GetFromJsonAsync<RecipeResponse>(
            $"/api/recipes/{created.Id}");
        Assert.NotNull(afterVisibilityUpdate);
        Assert.Equal(updated.RevisionId, afterVisibilityUpdate.RevisionId);
        Assert.Equal(2, afterVisibilityUpdate.RevisionNumber);
        Assert.Equal("Private", afterVisibilityUpdate.Visibility);

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
    public async Task RecipeListShouldFilterAndPaginate()
    {
        using HttpClient httpClient = _factory.CreateClient();
        var client = new ApiTestClient(httpClient);
        await client.RegisterAsync(TestEmail.Create("recipe-filters"));

        await CreateRecipeAsync(httpClient, CreateRequest("Быстрая паста", "Private", ["ужин", "быстро"]));
        await CreateRecipeAsync(
            httpClient,
            CreateRequest("Овсянка", "Private", ["завтрак"], category: "Breakfast"));

        RecipeListItemResponse[]? search = await httpClient.GetFromJsonAsync<RecipeListItemResponse[]>(
            "/api/recipes?search=паста");
        TagResponse[]? catalogTags = await httpClient.GetFromJsonAsync<TagResponse[]>(
            "/api/tags?search=быстро");
        Assert.NotNull(catalogTags);
        TagResponse catalogTag = Assert.Single(catalogTags);
        TagResponse[]? breakfastTags = await httpClient.GetFromJsonAsync<TagResponse[]>(
            "/api/tags?search=завтрак");
        Assert.NotNull(breakfastTags);
        TagResponse breakfastTag = Assert.Single(breakfastTags);
        RecipeListItemResponse[]? tags = await httpClient.GetFromJsonAsync<RecipeListItemResponse[]>(
            $"/api/recipes?tagIds={catalogTag.Id}&tagIds={breakfastTag.Id}");
        RecipeListItemResponse[]? category = await httpClient.GetFromJsonAsync<RecipeListItemResponse[]>(
            "/api/recipes?category=Breakfast");
        RecipeListItemResponse[]? firstPage = await httpClient.GetFromJsonAsync<RecipeListItemResponse[]>(
            "/api/recipes?page=1&pageSize=1");
        RecipeListItemResponse[]? secondPage = await httpClient.GetFromJsonAsync<RecipeListItemResponse[]>(
            "/api/recipes?page=2&pageSize=1");
        Assert.NotNull(search);
        Assert.NotNull(tags);
        Assert.NotNull(category);
        Assert.NotNull(firstPage);
        Assert.NotNull(secondPage);
        Assert.Equal("Быстрая паста", Assert.Single(search).Title);
        Assert.Equal(
            ["Быстрая паста", "Овсянка"],
            [.. tags.Select(recipe => recipe.Title).OrderBy(title => title)]);
        Assert.Equal("Овсянка", Assert.Single(category).Title);
        Assert.NotEqual(Assert.Single(firstPage).Id, Assert.Single(secondPage).Id);
        Assert.Equal("быстро", catalogTag.Name);
        Assert.Equal("User", catalogTag.Kind);
        Assert.Equal("Confirmed", catalogTag.Status);
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
        IReadOnlyCollection<string>? tags = null,
        string category = "MainCourse") =>
        new(
            title,
            "Описание рецепта",
            2,
            category,
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
