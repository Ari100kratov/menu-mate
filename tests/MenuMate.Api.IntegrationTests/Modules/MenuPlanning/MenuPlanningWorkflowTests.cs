using System.Net;
using System.Net.Http.Json;
using MenuMate.Contracts.MenuPlanning;

namespace MenuMate.Api.IntegrationTests;

public sealed class MenuPlanningWorkflowTests : IAsyncLifetime, IDisposable
{
    private static readonly DateOnly StartDate = new(2026, 6, 1);
    private readonly MenuMateApiFactory _factory = new();

    public Task InitializeAsync() => _factory.InitializeAsync();

    public Task DisposeAsync() => _factory.DisposeAsync();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task RegistrationShouldCreateDefaultMealSlotsAndSupportItemLifecycle()
    {
        using HttpClient httpClient = _factory.CreateClient();
        var client = new ApiTestClient(httpClient);
        await client.RegisterAsync(TestEmail.Create("menu-calendar-owner"));

        MenuCalendarResponse emptyCalendar = await GetCalendarAsync(httpClient);
        Assert.Empty(emptyCalendar.Items);
        Assert.Equal(["Завтрак", "Обед", "Ужин"], emptyCalendar.MealSlots.Select(slot => slot.Name));

        MealSlotResponse breakfast = emptyCalendar.MealSlots.First();
        MenuCalendarItemResponse created = await AddTextItemAsync(httpClient, breakfast.Id);

        HttpResponseMessage updateResponse = await httpClient.PutAsJsonAsync(
            $"/api/menu-calendar/items/{created.Id}",
            new UpdateMenuCalendarItemRequest(
                new DateOnly(2026, 6, 2),
                breakfast.Id,
                "Овсянка с ягодами",
                2,
                "Без сахара"));
        updateResponse.EnsureSuccessStatusCode();

        MenuCalendarResponse updatedCalendar = await GetCalendarAsync(httpClient);
        MenuCalendarItemResponse updated = Assert.Single(updatedCalendar.Items);
        Assert.Equal(new DateOnly(2026, 6, 2), updated.Date);
        Assert.Equal("Овсянка с ягодами", updated.Text);
        Assert.Equal(2, updated.Servings);

        HttpResponseMessage removeResponse = await httpClient.DeleteAsync(
            RelativeUri($"/api/menu-calendar/items/{created.Id}"));
        removeResponse.EnsureSuccessStatusCode();
        Assert.Empty((await GetCalendarAsync(httpClient)).Items);
    }

    [Fact]
    public async Task MealSlotsShouldBeRenamedAddedAndReordered()
    {
        using HttpClient httpClient = _factory.CreateClient();
        var client = new ApiTestClient(httpClient);
        await client.RegisterAsync(TestEmail.Create("meal-slot-settings"));

        MealSlotResponse[] slots = [.. (await GetCalendarAsync(httpClient)).MealSlots];
        MealSlotResponse breakfast = slots[0];

        HttpResponseMessage renameResponse = await httpClient.PutAsJsonAsync(
            $"/api/menu-calendar/meal-slots/{breakfast.Id}",
            new UpdateMealSlotRequest("Поздний завтрак"));
        renameResponse.EnsureSuccessStatusCode();

        HttpResponseMessage createResponse = await httpClient.PostAsJsonAsync(
            "/api/menu-calendar/meal-slots",
            new CreateMealSlotRequest("Полдник"));
        createResponse.EnsureSuccessStatusCode();
        MealSlotResponse[]? withCustom = await createResponse.Content.ReadFromJsonAsync<MealSlotResponse[]>();
        Assert.NotNull(withCustom);
        MealSlotResponse custom = Assert.Single(withCustom, slot => slot.Name == "Полдник");

        Guid[] reversedIds = [.. withCustom.Reverse().Select(slot => slot.Id)];
        HttpResponseMessage reorderResponse = await httpClient.PutAsJsonAsync(
            "/api/menu-calendar/meal-slots/order",
            new ReorderMealSlotsRequest(reversedIds));
        reorderResponse.EnsureSuccessStatusCode();
        MealSlotResponse[]? reordered = await reorderResponse.Content.ReadFromJsonAsync<MealSlotResponse[]>();
        Assert.NotNull(reordered);
        Assert.Equal(reversedIds, reordered.Select(slot => slot.Id).ToArray());

        MenuCalendarItemResponse customSlotItem = await AddTextItemAsync(httpClient, custom.Id);

        HttpResponseMessage deleteResponse = await httpClient.DeleteAsync(
            RelativeUri($"/api/menu-calendar/meal-slots/{custom.Id}"));
        deleteResponse.EnsureSuccessStatusCode();

        MenuCalendarResponse afterDelete = await GetCalendarAsync(httpClient);
        Assert.DoesNotContain(afterDelete.MealSlots, slot => slot.Id == custom.Id);
        Assert.DoesNotContain(afterDelete.Items, item => item.Id == customSlotItem.Id);
    }

    [Fact]
    public async Task DefaultMealSlotsShouldBeSeparateOwnedEntitiesForEachUser()
    {
        using HttpClient firstHttpClient = _factory.CreateClient();
        using HttpClient secondHttpClient = _factory.CreateClient();
        var first = new ApiTestClient(firstHttpClient);
        var second = new ApiTestClient(secondHttpClient);
        await first.RegisterAsync(TestEmail.Create("meal-slot-first-user"));
        await second.RegisterAsync(TestEmail.Create("meal-slot-second-user"));

        MealSlotResponse firstBreakfast = (await GetCalendarAsync(firstHttpClient)).MealSlots.First();
        MealSlotResponse secondBreakfast = (await GetCalendarAsync(secondHttpClient)).MealSlots.First();
        Assert.NotEqual(firstBreakfast.Id, secondBreakfast.Id);

        HttpResponseMessage firstRenameResponse = await firstHttpClient.PutAsJsonAsync(
            $"/api/menu-calendar/meal-slots/{firstBreakfast.Id}",
            new UpdateMealSlotRequest("Ранний завтрак"));
        HttpResponseMessage secondRenameResponse = await secondHttpClient.PutAsJsonAsync(
            $"/api/menu-calendar/meal-slots/{secondBreakfast.Id}",
            new UpdateMealSlotRequest("Поздний завтрак"));
        firstRenameResponse.EnsureSuccessStatusCode();
        secondRenameResponse.EnsureSuccessStatusCode();

        Assert.Contains((await GetCalendarAsync(firstHttpClient)).MealSlots, slot => slot.Name == "Ранний завтрак");
        Assert.Contains((await GetCalendarAsync(secondHttpClient)).MealSlots, slot => slot.Name == "Поздний завтрак");
    }

    [Fact]
    public async Task RemovingForeignCalendarItemShouldReturnForbiddenProblemDetails()
    {
        using HttpClient ownerHttpClient = _factory.CreateClient();
        using HttpClient otherHttpClient = _factory.CreateClient();
        var owner = new ApiTestClient(ownerHttpClient);
        var other = new ApiTestClient(otherHttpClient);
        await owner.RegisterAsync(TestEmail.Create("menu-calendar-first-user"));
        await other.RegisterAsync(TestEmail.Create("menu-calendar-second-user"));

        MealSlotResponse breakfast = (await GetCalendarAsync(ownerHttpClient)).MealSlots.First();
        MenuCalendarItemResponse item = await AddTextItemAsync(ownerHttpClient, breakfast.Id);

        HttpResponseMessage response = await otherHttpClient.DeleteAsync(
            RelativeUri($"/api/menu-calendar/items/{item.Id}"));

        await ProblemDetailsAssert.HasProblemAsync(
            response,
            HttpStatusCode.Forbidden,
            "MenuPlanning.AccessDenied");
    }

    [Fact]
    public async Task ClearingRangeShouldOnlyRemoveItemsInsideRange()
    {
        using HttpClient httpClient = _factory.CreateClient();
        var client = new ApiTestClient(httpClient);
        await client.RegisterAsync(TestEmail.Create("menu-calendar-clear"));

        MealSlotResponse breakfast = (await GetCalendarAsync(httpClient)).MealSlots.First();
        await AddTextItemAsync(httpClient, breakfast.Id);

        HttpResponseMessage response = await httpClient.DeleteAsync(
            RelativeUri("/api/menu-calendar?startDate=2026-06-01&endDate=2026-06-07"));
        response.EnsureSuccessStatusCode();

        Assert.Empty((await GetCalendarAsync(httpClient)).Items);
    }

    private static async Task<MenuCalendarResponse> GetCalendarAsync(HttpClient client)
    {
        MenuCalendarResponse? calendar = await client.GetFromJsonAsync<MenuCalendarResponse>(
            "/api/menu-calendar?startDate=2026-06-01&endDate=2026-06-07");
        Assert.NotNull(calendar);
        return calendar;
    }

    private static async Task<MenuCalendarItemResponse> AddTextItemAsync(HttpClient client, Guid mealSlotId)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/menu-calendar/items",
            new CreateMenuCalendarItemRequest(
                StartDate,
                mealSlotId,
                null,
                null,
                "Овсянка",
                1,
                null));
        response.EnsureSuccessStatusCode();

        MenuCalendarItemResponse? item = await response.Content.ReadFromJsonAsync<MenuCalendarItemResponse>();
        Assert.NotNull(item);
        return item;
    }

    private static Uri RelativeUri(string path) => new(path, UriKind.Relative);
}
