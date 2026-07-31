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

        HttpResponseMessage favoriteResponse = await httpClient.PostAsync(
            RelativeUri($"/api/recipes/{created.Id}/favorite"),
            content: null);
        favoriteResponse.EnsureSuccessStatusCode();

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
        Assert.Equal(updated.RevisionId, updated.SavedRevisionId);
        Assert.True(updated.IsDisplayedRevisionSaved);
        Assert.Equal("Current", updated.RevisionState);

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

        RecipeListPageResponse? favorites = await httpClient.GetFromJsonAsync<RecipeListPageResponse>(
            "/api/recipes?favoritesOnly=true");
        Assert.NotNull(favorites);
        Assert.Equal(created.Id, Assert.Single(favorites.Items).Id);
        Assert.Equal(1, favorites.TotalCount);

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
    public async Task RecipeAdviceShouldBeVersionedAndAvailableInRecipeDetails()
    {
        using HttpClient httpClient = _factory.CreateClient();
        var client = new ApiTestClient(httpClient);
        await client.RegisterAsync(TestEmail.Create("recipe-advice"));

        const string initialAdvice = "Дайте блюду настояться 10 минут.\nПодавайте горячим.";
        RecipeResponse created = await CreateRecipeAsync(
            httpClient,
            CreateRequest("Паста", "Private", advice: initialAdvice));

        Assert.Equal(initialAdvice, created.Advice);

        const string updatedAdvice = "Добавьте зелень перед подачей.";
        HttpResponseMessage updateResponse = await httpClient.PutAsJsonAsync(
            $"/api/recipes/{created.Id}",
            CreateRequest("Паста", "Private", advice: updatedAdvice));
        updateResponse.EnsureSuccessStatusCode();

        RecipeResponse? updated = await httpClient.GetFromJsonAsync<RecipeResponse>($"/api/recipes/{created.Id}");
        Assert.NotNull(updated);
        Assert.Equal(2, updated.RevisionNumber);
        Assert.Equal(updatedAdvice, updated.Advice);

        RecipeResponse? initialRevision = await httpClient.GetFromJsonAsync<RecipeResponse>(
            $"/api/recipes/{created.Id}?revisionId={created.RevisionId}");
        Assert.NotNull(initialRevision);
        Assert.Equal(initialAdvice, initialRevision.Advice);
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

        RecipeListPageResponse? search = await httpClient.GetFromJsonAsync<RecipeListPageResponse>(
            "/api/recipes?search=паста");
        TagResponse[]? catalogTags = await httpClient.GetFromJsonAsync<TagResponse[]>(
            "/api/tags?search=быстро");
        Assert.NotNull(catalogTags);
        TagResponse catalogTag = Assert.Single(catalogTags);
        TagResponse[]? breakfastTags = await httpClient.GetFromJsonAsync<TagResponse[]>(
            "/api/tags?search=завтрак");
        Assert.NotNull(breakfastTags);
        TagResponse breakfastTag = Assert.Single(breakfastTags);
        RecipeListPageResponse? tags = await httpClient.GetFromJsonAsync<RecipeListPageResponse>(
            $"/api/recipes?tagIds={catalogTag.Id}&tagIds={breakfastTag.Id}");
        RecipeListPageResponse? category = await httpClient.GetFromJsonAsync<RecipeListPageResponse>(
            "/api/recipes?category=Breakfast");
        RecipeListPageResponse? firstPage = await httpClient.GetFromJsonAsync<RecipeListPageResponse>(
            "/api/recipes?page=1&pageSize=1");
        RecipeListPageResponse? secondPage = await httpClient.GetFromJsonAsync<RecipeListPageResponse>(
            "/api/recipes?page=2&pageSize=1");
        Assert.NotNull(search);
        Assert.NotNull(tags);
        Assert.NotNull(category);
        Assert.NotNull(firstPage);
        Assert.NotNull(secondPage);
        Assert.Equal("Быстрая паста", Assert.Single(search.Items).Title);
        Assert.Equal(
            ["Быстрая паста", "Овсянка"],
            [.. tags.Items.Select(recipe => recipe.Title).OrderBy(title => title)]);
        Assert.Equal("Овсянка", Assert.Single(category.Items).Title);
        Assert.NotEqual(Assert.Single(firstPage.Items).Id, Assert.Single(secondPage.Items).Id);
        Assert.Equal(1, search.TotalCount);
        Assert.Equal(2, tags.TotalCount);
        Assert.Equal(1, category.TotalCount);
        Assert.Equal(2, firstPage.TotalCount);
        Assert.Equal(2, secondPage.TotalCount);
        Assert.Equal("быстро", catalogTag.Name);
        Assert.Equal("User", catalogTag.Kind);
        Assert.Equal("Confirmed", catalogTag.Status);
    }

    [Fact]
    public async Task RecipeSearchShouldIgnoreCaseAndTreatYoAsYe()
    {
        using HttpClient httpClient = _factory.CreateClient();
        var client = new ApiTestClient(httpClient);
        await client.RegisterAsync(TestEmail.Create("recipe-yo-search"));

        await CreateRecipeAsync(
            httpClient,
            CreateRequest("Тёртая свёкла", "Private") with
            {
                Description = "Зелёный салат"
            });

        RecipeListPageResponse? byTitle = await httpClient.GetFromJsonAsync<RecipeListPageResponse>(
            $"/api/recipes?search={Uri.EscapeDataString("ТЕРТАЯ СВЕКЛА")}");
        RecipeListPageResponse? byDescription = await httpClient.GetFromJsonAsync<RecipeListPageResponse>(
            $"/api/recipes?search={Uri.EscapeDataString("зЕЛЕный")}");

        Assert.NotNull(byTitle);
        Assert.NotNull(byDescription);
        Assert.Equal("Тёртая свёкла", Assert.Single(byTitle.Items).Title);
        Assert.Equal("Тёртая свёкла", Assert.Single(byDescription.Items).Title);
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
        string category = "MainCourse",
        string? advice = null) =>
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
            tags ?? [],
            advice);

    private static Uri RelativeUri(string path) => new(path, UriKind.Relative);
}
