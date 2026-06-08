using System.Net;
using System.Net.Http.Json;
using MenuMate.Contracts.MenuPlanning;
using MenuMate.Contracts.Recipes;
using MenuMate.Contracts.ShoppingLists;

namespace MenuMate.Api.IntegrationTests;

public sealed class ShoppingListsWorkflowTests : IAsyncLifetime, IDisposable
{
    private readonly MenuMateApiFactory _factory = new();

    public Task InitializeAsync() => _factory.InitializeAsync();

    public Task DisposeAsync() => _factory.DisposeAsync();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task GenerateShoppingListFromMenuShouldScaleGroupAndUpdateItems()
    {
        using HttpClient httpClient = _factory.CreateClient();
        var client = new ApiTestClient(httpClient);

        await client.RegisterAsync(TestEmail.Create("shopping"));

        RecipeResponse recipe = await CreateRecipeAsync(httpClient);
        MenuPlanResponse menuPlan = await CreateMenuPlanAsync(httpClient);
        await AddRecipeToMenuPlanAsync(httpClient, menuPlan.Id, recipe.Id, recipe.CurrentRevisionId);
        await UpdateRecipeAfterPlanningAsync(httpClient, recipe.Id);

        ShoppingListResponse shoppingList = await GenerateShoppingListAsync(httpClient, menuPlan.Id);

        ShoppingListItemResponse item = Assert.Single(Assert.Single(shoppingList.Categories).Items);
        Assert.Equal("Rice", item.Name);
        Assert.Equal(1000m, item.Amount);
        Assert.Equal("Gram", item.Unit);
        Assert.Contains("1000", shoppingList.Text, StringComparison.Ordinal);

        HttpResponseMessage stateResponse = await httpClient.PatchAsJsonAsync(
            $"/api/shopping-lists/{shoppingList.Id}/items/{item.Id}/state",
            new ShoppingListItemStateRequest(true, false));

        stateResponse.EnsureSuccessStatusCode();
        ShoppingListResponse? updated = await stateResponse.Content.ReadFromJsonAsync<ShoppingListResponse>();
        Assert.NotNull(updated);
        Assert.True(Assert.Single(Assert.Single(updated.Categories).Items).IsPurchased);

        ShoppingListItemResponse manualItem = await AddManualItemAsync(httpClient, shoppingList.Id);
        ShoppingListResponse listWithUpdatedManualItem = await UpdateManualItemAsync(
            httpClient,
            shoppingList.Id,
            manualItem.Id);

        ShoppingListItemResponse updatedManualItem = Assert.Single(
            listWithUpdatedManualItem.Categories.SelectMany(category => category.Items),
            item => item.Name == "Oat milk");

        Assert.Equal(2m, updatedManualItem.Amount);
        Assert.Equal("Liter", updatedManualItem.Unit);

        HttpResponseMessage removeItemResponse = await httpClient.DeleteAsync(
            RelativeUri($"/api/shopping-lists/{shoppingList.Id}/items/{manualItem.Id}"));

        removeItemResponse.EnsureSuccessStatusCode();

        HttpResponseMessage deleteListResponse = await httpClient.DeleteAsync(
            RelativeUri($"/api/shopping-lists/{shoppingList.Id}"));
        deleteListResponse.EnsureSuccessStatusCode();

        HttpResponseMessage getDeletedResponse = await httpClient.GetAsync(
            RelativeUri($"/api/shopping-lists/{shoppingList.Id}"));
        await ProblemDetailsAssert.HasProblemAsync(getDeletedResponse, HttpStatusCode.NotFound, "ShoppingLists.NotFound");
    }

    private static async Task<RecipeResponse> CreateRecipeAsync(HttpClient client)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/recipes/",
            new CreateRecipeRequest(
                "Rice bowl",
                null,
                2,
                "MainCourse",
                "Private",
                30,
                10,
                null,
                [
                    new RecipeIngredientRequest(
                        null,
                        "Rice",
                        0.5m,
                        "Kilogram",
                        "Exact",
                        "GrainsAndPasta",
                        null,
                        false)
                ],
                [new PreparationStepRequest("Cook rice")],
                ["dinner"]));

        response.EnsureSuccessStatusCode();

        RecipeResponse? recipe = await response.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(recipe);
        return recipe;
    }

    private static async Task<MenuPlanResponse> CreateMenuPlanAsync(HttpClient client)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/menu-plans/",
            new CreateMenuPlanRequest(
                "Week",
                new DateOnly(2026, 6, 1),
                new DateOnly(2026, 6, 7)));

        response.EnsureSuccessStatusCode();

        MenuPlanResponse? menuPlan = await response.Content.ReadFromJsonAsync<MenuPlanResponse>();
        Assert.NotNull(menuPlan);
        return menuPlan;
    }

    private static async Task UpdateRecipeAfterPlanningAsync(HttpClient client, Guid recipeId)
    {
        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/recipes/{recipeId}",
            new UpdateRecipeRequest(
                "Rice bowl updated",
                null,
                2,
                "MainCourse",
                "Private",
                30,
                10,
                null,
                [
                    new RecipeIngredientRequest(
                        null,
                        "Beans",
                        1m,
                        "Kilogram",
                        "Exact",
                        "Grocery",
                        null,
                        false)
                ],
                [new PreparationStepRequest("Cook beans")],
                []));

        response.EnsureSuccessStatusCode();
    }

    private static async Task AddRecipeToMenuPlanAsync(
        HttpClient client,
        Guid menuPlanId,
        Guid recipeId,
        Guid recipeRevisionId)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/api/menu-plans/{menuPlanId}/items",
            new CreateMenuPlanItemRequest(
                new DateOnly(2026, 6, 1),
                "Dinner",
                recipeId,
                recipeRevisionId,
                "Rice bowl",
                null,
                4,
                null));

        response.EnsureSuccessStatusCode();
    }

    private static async Task<ShoppingListResponse> GenerateShoppingListAsync(HttpClient client, Guid menuPlanId)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/shopping-lists/",
            new GenerateShoppingListRequest(menuPlanId, []));

        response.EnsureSuccessStatusCode();

        ShoppingListResponse? shoppingList = await response.Content.ReadFromJsonAsync<ShoppingListResponse>();
        Assert.NotNull(shoppingList);
        return shoppingList;
    }

    private static async Task<ShoppingListItemResponse> AddManualItemAsync(HttpClient client, Guid shoppingListId)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/api/shopping-lists/{shoppingListId}/items",
            new ShoppingListItemRequest(
                null,
                "Milk",
                1m,
                "Liter",
                "Exact",
                "Dairy",
                "For coffee"));

        response.EnsureSuccessStatusCode();

        ShoppingListResponse? shoppingList = await response.Content.ReadFromJsonAsync<ShoppingListResponse>();
        Assert.NotNull(shoppingList);

        return Assert.Single(
            shoppingList.Categories.SelectMany(category => category.Items),
            item => item.Name == "Milk");
    }

    private static async Task<ShoppingListResponse> UpdateManualItemAsync(
        HttpClient client,
        Guid shoppingListId,
        Guid itemId)
    {
        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/shopping-lists/{shoppingListId}/items/{itemId}",
            new ShoppingListItemRequest(
                null,
                "Oat milk",
                2m,
                "Liter",
                "Exact",
                "Dairy",
                "Unsweetened"));

        response.EnsureSuccessStatusCode();

        ShoppingListResponse? shoppingList = await response.Content.ReadFromJsonAsync<ShoppingListResponse>();
        Assert.NotNull(shoppingList);
        return shoppingList;
    }

    private static Uri RelativeUri(string path) => new(path, UriKind.Relative);
}
