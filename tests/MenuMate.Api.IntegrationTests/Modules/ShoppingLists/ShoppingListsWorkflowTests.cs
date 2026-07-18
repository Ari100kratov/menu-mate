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
        Guid dinnerSlotId = await GetDinnerSlotIdAsync(httpClient);
        await AddRecipeToMenuAsync(httpClient, dinnerSlotId, recipe.Id, recipe.CurrentRevisionId);
        await UpdateRecipeAfterPlanningAsync(httpClient, recipe.Id);

        ShoppingListResponse shoppingList = await GenerateShoppingListAsync(httpClient);

        ShoppingListItemResponse item = Assert.Single(Assert.Single(shoppingList.Categories).Items);
        Assert.Equal("rice", item.Name);
        Assert.Equal(1000m, item.Amount);
        Assert.Equal("Gram", item.Unit);
        Assert.Contains("1000", shoppingList.Text, StringComparison.Ordinal);

        HttpResponseMessage stateResponse = await httpClient.PatchAsJsonAsync(
            $"/api/shopping-list/items/{item.Id}/checked",
            new ShoppingListItemStateRequest(true));

        stateResponse.EnsureSuccessStatusCode();
        ShoppingListResponse? updated = await stateResponse.Content.ReadFromJsonAsync<ShoppingListResponse>();
        Assert.NotNull(updated);
        Assert.True(Assert.Single(Assert.Single(updated.Categories).Items).IsPurchased);

        ShoppingListItemResponse manualItem = await AddManualItemAsync(httpClient);
        HttpResponseMessage manualStateResponse = await httpClient.PatchAsJsonAsync(
            $"/api/shopping-list/items/{manualItem.Id}/checked",
            new ShoppingListItemStateRequest(true));
        manualStateResponse.EnsureSuccessStatusCode();

        ShoppingListResponse listWithUpdatedManualItem = await UpdateManualItemAsync(
            httpClient,
            manualItem.Id);

        ShoppingListItemResponse updatedManualItem = Assert.Single(
            listWithUpdatedManualItem.Categories.SelectMany(category => category.Items),
            item => item.Name == "oat milk");
        ShoppingListItemResponse purchasedRice = Assert.Single(
            listWithUpdatedManualItem.Categories.SelectMany(category => category.Items),
            item => item.Name == "rice");

        Assert.Equal(2m, updatedManualItem.Amount);
        Assert.Equal("Liter", updatedManualItem.Unit);
        Assert.True(updatedManualItem.IsPurchased);
        Assert.True(purchasedRice.IsPurchased);

        HttpResponseMessage removeItemResponse = await httpClient.DeleteAsync(
            RelativeUri($"/api/shopping-list/items/{manualItem.Id}"));

        removeItemResponse.EnsureSuccessStatusCode();

        ShoppingListResponse replacedList = await GenerateShoppingListAsync(httpClient);
        Assert.Equal(shoppingList.Id, replacedList.Id);
        Assert.False(Assert.Single(Assert.Single(replacedList.Categories).Items).IsPurchased);
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

    private static async Task<Guid> GetDinnerSlotIdAsync(HttpClient client)
    {
        MenuCalendarResponse? calendar = await client.GetFromJsonAsync<MenuCalendarResponse>(
            "/api/menu-calendar?startDate=2026-06-01&endDate=2026-06-07");
        Assert.NotNull(calendar);
        return Assert.Single(calendar.MealSlots, slot => slot.Name == "Ужин").Id;
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
                        "Grocery",
                        null,
                        false)
                ],
                [new PreparationStepRequest("Cook beans")],
                []));

        response.EnsureSuccessStatusCode();
    }

    private static async Task AddRecipeToMenuAsync(
        HttpClient client,
        Guid mealSlotId,
        Guid recipeId,
        Guid recipeRevisionId)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/menu-calendar/items",
            new CreateMenuCalendarItemRequest(
                new DateOnly(2026, 6, 1),
                mealSlotId,
                recipeId,
                recipeRevisionId,
                "Rice bowl",
                null,
                4,
                null));

        response.EnsureSuccessStatusCode();
    }

    private static async Task<ShoppingListResponse> GenerateShoppingListAsync(HttpClient client)
    {
        MenuShoppingPreviewResponse? preview = await client.GetFromJsonAsync<MenuShoppingPreviewResponse>(
            "/api/shopping-list/menu-preview?startDate=2026-06-01&endDate=2026-06-07");
        Assert.NotNull(preview);
        MenuShoppingPreviewRecipeResponse recipe = Assert.Single(preview.Recipes);

        HttpResponseMessage response = await client.PutAsJsonAsync(
            "/api/shopping-list/from-menu",
            new GenerateShoppingListRequest(
                new DateOnly(2026, 6, 1),
                new DateOnly(2026, 6, 7),
                [new MenuShoppingSelectionRequest(
                    recipe.MenuItemId,
                    recipe.Servings,
                    [.. recipe.Ingredients.Select(ingredient => ingredient.IngredientId)])]));

        response.EnsureSuccessStatusCode();

        ShoppingListResponse? shoppingList = await response.Content.ReadFromJsonAsync<ShoppingListResponse>();
        Assert.NotNull(shoppingList);
        return shoppingList;
    }

    private static async Task<ShoppingListItemResponse> AddManualItemAsync(HttpClient client)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/shopping-list/items",
            new ShoppingListItemRequest(
                null,
                "Milk",
                1m,
                "Liter",
                "Dairy",
                "For coffee"));

        response.EnsureSuccessStatusCode();

        ShoppingListResponse? shoppingList = await response.Content.ReadFromJsonAsync<ShoppingListResponse>();
        Assert.NotNull(shoppingList);

        return Assert.Single(
            shoppingList.Categories.SelectMany(category => category.Items),
            item => item.Name == "milk");
    }

    private static async Task<ShoppingListResponse> UpdateManualItemAsync(
        HttpClient client,
        Guid itemId)
    {
        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/shopping-list/items/{itemId}",
            new ShoppingListItemRequest(
                null,
                "Oat milk",
                2m,
                "Liter",
                "Dairy",
                "Unsweetened"));

        response.EnsureSuccessStatusCode();

        ShoppingListResponse? shoppingList = await response.Content.ReadFromJsonAsync<ShoppingListResponse>();
        Assert.NotNull(shoppingList);
        return shoppingList;
    }

    private static Uri RelativeUri(string path) => new(path, UriKind.Relative);
}
