using System.Net;
using System.Net.Http.Json;
using MenuMate.Contracts.MenuPlanning;

namespace MenuMate.Api.IntegrationTests;

public sealed class MenuPlanningWorkflowTests : IAsyncLifetime, IDisposable
{
    private readonly MenuMateApiFactory _factory = new();

    public Task InitializeAsync() => _factory.InitializeAsync();

    public Task DisposeAsync() => _factory.DisposeAsync();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task MenuPlanShouldBeUpdatedAndDeletedByOwner()
    {
        using HttpClient httpClient = _factory.CreateClient();
        var client = new ApiTestClient(httpClient);

        await client.RegisterAsync(TestEmail.Create("menu-plan-owner"));

        MenuPlanResponse created = await CreateMenuPlanAsync(httpClient, "Week");
        MenuPlanResponse withItem = await AddTextItemAsync(httpClient, created.Id);

        Assert.Single(withItem.Items);

        HttpResponseMessage updateResponse = await httpClient.PutAsJsonAsync(
            $"/api/menu-plans/{created.Id}",
            new UpdateMenuPlanRequest(
                "Updated week",
                new DateOnly(2026, 6, 1),
                new DateOnly(2026, 6, 8)));

        updateResponse.EnsureSuccessStatusCode();

        MenuPlanResponse? updated = await updateResponse.Content.ReadFromJsonAsync<MenuPlanResponse>();
        Assert.NotNull(updated);
        Assert.Equal("Updated week", updated.Name);
        Assert.Equal(new DateOnly(2026, 6, 8), updated.EndDate);

        Guid itemId = Assert.Single(updated.Items).Id;
        HttpResponseMessage removeItemResponse = await httpClient.DeleteAsync(
            RelativeUri($"/api/menu-plans/{created.Id}/items/{itemId}"));

        removeItemResponse.EnsureSuccessStatusCode();

        MenuPlanResponse? afterItemRemoval = await httpClient.GetFromJsonAsync<MenuPlanResponse>(
            $"/api/menu-plans/{created.Id}");

        Assert.NotNull(afterItemRemoval);
        Assert.Empty(afterItemRemoval.Items);

        HttpResponseMessage deleteResponse = await httpClient.DeleteAsync(
            RelativeUri($"/api/menu-plans/{created.Id}"));
        deleteResponse.EnsureSuccessStatusCode();

        HttpResponseMessage getDeletedResponse = await httpClient.GetAsync(
            RelativeUri($"/api/menu-plans/{created.Id}"));
        await ProblemDetailsAssert.HasProblemAsync(getDeletedResponse, HttpStatusCode.NotFound, "MenuPlanning.NotFound");
    }

    [Fact]
    public async Task RemovingMissingMenuPlanItemShouldReturnNotFoundProblemDetails()
    {
        using HttpClient httpClient = _factory.CreateClient();
        var client = new ApiTestClient(httpClient);

        await client.RegisterAsync(TestEmail.Create("menu-plan-missing-item"));

        MenuPlanResponse menuPlan = await CreateMenuPlanAsync(httpClient, "Week");

        HttpResponseMessage response = await httpClient.DeleteAsync(
            RelativeUri($"/api/menu-plans/{menuPlan.Id}/items/{Guid.CreateVersion7()}"));

        await ProblemDetailsAssert.HasProblemAsync(response, HttpStatusCode.NotFound, "MenuPlanning.ItemNotFound");
    }

    [Fact]
    public async Task MenuPlansListShouldReturnPlansWithItems()
    {
        using HttpClient httpClient = _factory.CreateClient();
        var client = new ApiTestClient(httpClient);

        await client.RegisterAsync(TestEmail.Create("menu-plan-list"));

        MenuPlanResponse created = await CreateMenuPlanAsync(httpClient, "Week");
        await AddTextItemAsync(httpClient, created.Id);

        MenuPlanResponse[]? menuPlans = await httpClient.GetFromJsonAsync<MenuPlanResponse[]>("/api/menu-plans/");

        Assert.NotNull(menuPlans);
        MenuPlanResponse menuPlan = Assert.Single(menuPlans);
        Assert.Equal(created.Id, menuPlan.Id);
        MenuPlanItemResponse item = Assert.Single(menuPlan.Items);
        Assert.Equal("Breakfast", item.MealType);
        Assert.Equal("Oatmeal", item.Text);
    }

    [Fact]
    public async Task MenuPlanOwnedByAnotherUserShouldReturnForbiddenProblemDetails()
    {
        using HttpClient ownerHttpClient = _factory.CreateClient();
        using HttpClient otherHttpClient = _factory.CreateClient();
        var owner = new ApiTestClient(ownerHttpClient);
        var other = new ApiTestClient(otherHttpClient);

        await owner.RegisterAsync(TestEmail.Create("menu-plan-first-user"));
        await other.RegisterAsync(TestEmail.Create("menu-plan-second-user"));

        MenuPlanResponse menuPlan = await CreateMenuPlanAsync(ownerHttpClient, "Owner week");

        HttpResponseMessage updateResponse = await otherHttpClient.PutAsJsonAsync(
            $"/api/menu-plans/{menuPlan.Id}",
            new UpdateMenuPlanRequest(
                "Other week",
                new DateOnly(2026, 6, 1),
                new DateOnly(2026, 6, 7)));

        await ProblemDetailsAssert.HasProblemAsync(updateResponse, HttpStatusCode.Forbidden, "MenuPlanning.AccessDenied");

        HttpResponseMessage deleteResponse = await otherHttpClient.DeleteAsync(
            RelativeUri($"/api/menu-plans/{menuPlan.Id}"));
        await ProblemDetailsAssert.HasProblemAsync(deleteResponse, HttpStatusCode.Forbidden, "MenuPlanning.AccessDenied");
    }

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

    private static async Task<MenuPlanResponse> AddTextItemAsync(HttpClient client, Guid menuPlanId)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/api/menu-plans/{menuPlanId}/items",
            new CreateMenuPlanItemRequest(
                new DateOnly(2026, 6, 1),
                "Breakfast",
                null,
                null,
                null,
                "Oatmeal",
                1,
                null));

        response.EnsureSuccessStatusCode();

        MenuPlanResponse? menuPlan = await response.Content.ReadFromJsonAsync<MenuPlanResponse>();
        Assert.NotNull(menuPlan);
        return menuPlan;
    }

    private static Uri RelativeUri(string path) => new(path, UriKind.Relative);
}
