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
        ProductResponse[] exactProducts =
        [
            .. products.Where(product =>
                string.Equals(product.Name, "Курица", StringComparison.OrdinalIgnoreCase))
        ];
        Assert.Equal(2, exactProducts.Length);
        Assert.Equal(2, exactProducts.Select(product => product.Id).Distinct().Count());
        Assert.Equal(
            ["MeatAndPoultry", "Other"],
            exactProducts.Select(product => product.Category).Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ExactProductMatchShouldBeRankedBeforeSubstringMatches()
    {
        using HttpClient httpClient = _factory.CreateClient();
        var client = new ApiTestClient(httpClient);
        await client.RegisterAsync(TestEmail.Create("product-ranking"));

        ProductResponse[]? products = await httpClient.GetFromJsonAsync<ProductResponse[]>(
            "/api/products?search=соль");

        Assert.NotNull(products);
        Assert.NotEmpty(products);
        Assert.Equal("соль", products[0].Name, ignoreCase: true);
        Assert.Contains(
            products,
            product => product.Name.Contains("фасоль", StringComparison.OrdinalIgnoreCase));
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
