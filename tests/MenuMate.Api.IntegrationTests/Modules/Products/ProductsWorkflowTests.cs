using System.Net.Http.Json;
using MenuMate.Contracts.Products;
using MenuMate.Contracts.Recipes;

namespace MenuMate.Api.IntegrationTests;

public sealed class ProductsWorkflowTests : IAsyncLifetime, IDisposable
{
    private readonly MenuMateApiFactory _factory = new();

    public Task InitializeAsync() => _factory.InitializeAsync();

    public Task DisposeAsync() => _factory.DisposeAsync();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task ProductsCreatedByRecipesShouldBeSearchableAndDistinctByCategory()
    {
        using HttpClient httpClient = _factory.CreateClient();
        var client = new ApiTestClient(httpClient);
        await client.RegisterAsync(TestEmail.Create("products"));

        await CreateRecipeAsync(httpClient, "Курица как мясо", "Курица", "MeatAndPoultry");
        await CreateRecipeAsync(httpClient, "Курица как другое", "Курица", "Other");

        ProductResponse[]? products = await httpClient.GetFromJsonAsync<ProductResponse[]>(
            "/api/products?search=кур");

        Assert.NotNull(products);
        Assert.Equal(2, products.Length);
        Assert.Equal(2, products.Select(product => product.Id).Distinct().Count());
        Assert.Equal(
            ["MeatAndPoultry", "Other"],
            products.Select(product => product.Category).Order(StringComparer.Ordinal));
    }

    private static async Task CreateRecipeAsync(
        HttpClient client,
        string title,
        string productName,
        string category)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/recipes/",
            new CreateRecipeRequest(
                title,
                null,
                2,
                "MainCourse",
                "Private",
                null,
                null,
                null,
                [
                    new RecipeIngredientRequest(
                        null,
                        productName,
                        1m,
                        "Piece",
                        category,
                        null,
                        false)
                ],
                [new PreparationStepRequest("Приготовить")],
                []));

        response.EnsureSuccessStatusCode();
    }
}
