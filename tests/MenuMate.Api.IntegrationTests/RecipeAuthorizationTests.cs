using System.Net;
using System.Net.Http.Json;
using MenuMate.Contracts.Recipes;

namespace MenuMate.Api.IntegrationTests;

public sealed class RecipeAuthorizationTests : IAsyncLifetime, IDisposable
{
    private readonly MenuMateApiFactory _factory = new();

    public Task InitializeAsync() => _factory.InitializeAsync();

    public Task DisposeAsync() => _factory.DisposeAsync();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task UpdateRecipeOwnedByAnotherUserShouldReturnForbiddenProblemDetails()
    {
        using HttpClient ownerHttpClient = _factory.CreateClient();
        using HttpClient otherHttpClient = _factory.CreateClient();
        var owner = new ApiTestClient(ownerHttpClient);
        var other = new ApiTestClient(otherHttpClient);

        await owner.RegisterAsync(TestEmail.Create("recipe-owner"));
        await other.RegisterAsync(TestEmail.Create("recipe-other"));

        RecipeResponse recipe = await CreateRecipeAsync(ownerHttpClient, "Pasta");

        HttpResponseMessage response = await otherHttpClient.PutAsJsonAsync(
            $"/api/recipes/{recipe.Id}",
            CreateRecipeRequest("Pasta updated"));

        await ProblemDetailsAssert.HasProblemAsync(response, HttpStatusCode.Forbidden, "Recipes.AccessDenied");
    }

    [Fact]
    public async Task InvalidRecipePayloadShouldReturnValidationProblemDetails()
    {
        using HttpClient httpClient = _factory.CreateClient();
        var client = new ApiTestClient(httpClient);

        await client.RegisterAsync(TestEmail.Create("recipe-validation"));

        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            "/api/recipes/",
            CreateRecipeRequest(""));

        await ProblemDetailsAssert.HasProblemAsync(response, HttpStatusCode.BadRequest, "Recipes.EmptyTitle");
    }

    [Fact]
    public async Task UploadRecipeImageOwnedByAnotherUserShouldReturnForbiddenProblemDetails()
    {
        using HttpClient ownerHttpClient = _factory.CreateClient();
        using HttpClient otherHttpClient = _factory.CreateClient();
        var owner = new ApiTestClient(ownerHttpClient);
        var other = new ApiTestClient(otherHttpClient);

        await owner.RegisterAsync(TestEmail.Create("recipe-image-owner"));
        await other.RegisterAsync(TestEmail.Create("recipe-image-other"));

        RecipeResponse recipe = await CreateRecipeAsync(ownerHttpClient, "Pizza");
        using var content = new MultipartFormDataContent();
        using var imageContent = new ByteArrayContent([137, 80, 78, 71]);
        using var scopeContent = new StringContent("Cover");
        imageContent.Headers.ContentType = new("image/png");
        content.Add(imageContent, "file", "cover.png");
        content.Add(scopeContent, "scope");

        HttpResponseMessage response = await otherHttpClient.PostAsync(
            new Uri($"/api/recipes/{recipe.Id}/images", UriKind.Relative),
            content);

        await ProblemDetailsAssert.HasProblemAsync(response, HttpStatusCode.Forbidden, "Recipes.AccessDenied");
    }

    private static async Task<RecipeResponse> CreateRecipeAsync(HttpClient client, string title)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/recipes/", CreateRecipeRequest(title));
        response.EnsureSuccessStatusCode();

        RecipeResponse? recipe = await response.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(recipe);
        return recipe;
    }

    private static CreateRecipeRequest CreateRecipeRequest(string title) =>
        new(
            title,
            "Simple dinner",
            2,
            "MainCourse",
            "Private",
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
            [new PreparationStepRequest("Boil pasta")],
            ["dinner"]);
}
