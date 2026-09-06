using System.Net;
using System.Net.Http.Json;
using MenuMate.Contracts.Auth;
using MenuMate.Contracts.MenuPlanning;
using MenuMate.Contracts.Recipes;
using MenuMate.Contracts.ShoppingLists;
using MenuMate.Modules.Auth.Application;
using MenuMate.Modules.Auth.Infrastructure.Database;
using MenuMate.Modules.MenuPlanning.Infrastructure.Database;
using MenuMate.Modules.Recipes.Infrastructure.Database;
using MenuMate.Modules.ShoppingLists.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MenuMate.Api.IntegrationTests;

public sealed class PrivacyAndAccountDeletionTests : IAsyncLifetime, IDisposable
{
    private const string Password = "Password123!";

    private readonly MenuMateApiFactory _factory = new();

    public Task InitializeAsync() => _factory.InitializeAsync();

    public Task DisposeAsync() => _factory.DisposeAsync();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task PrivacyPolicyShouldBePublicAndRequireCurrentVersion()
    {
        using HttpClient httpClient = _factory.CreateClient();
        PrivacyPolicyResponse? policy = await httpClient.GetFromJsonAsync<PrivacyPolicyResponse>(
            "/api/legal/privacy-policy");

        Assert.NotNull(policy);
        Assert.Equal(PrivacyPolicyDefaults.CurrentVersion, policy.Version);
        Assert.Equal("MenuMate Integration Tests", policy.OperatorName);
        Assert.Equal("privacy@menumate.test", policy.ContactEmail);

        HttpResponseMessage outdatedRegistration = await httpClient.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterUserRequest(
                TestEmail.Create("privacy-outdated"),
                "Пользователь",
                Password,
                "outdated"));

        await ProblemDetailsAssert.HasProblemAsync(
            outdatedRegistration,
            HttpStatusCode.BadRequest,
            "Auth.PrivacyPolicyOutdated");
    }

    [Fact]
    public async Task ExistingUserShouldAcceptCurrentPrivacyPolicy()
    {
        using HttpClient httpClient = _factory.CreateClient();
        var client = new ApiTestClient(httpClient);
        UserProfileResponse registered = await client.RegisterAsync(TestEmail.Create("privacy-existing"));

        await using (AsyncServiceScope scope = _factory.Services.CreateAsyncScope())
        {
            AuthDbContext dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""
                UPDATE auth.users
                SET privacy_policy_accepted_version = NULL,
                    privacy_policy_accepted_at = NULL
                WHERE id = {registered.Id};
                """);
        }

        UserProfileResponse? profile = await httpClient.GetFromJsonAsync<UserProfileResponse>("/api/auth/me");
        Assert.NotNull(profile);
        Assert.True(profile.RequiresPrivacyPolicyAcceptance);
        Assert.Null(profile.PrivacyPolicyAcceptedVersion);

        HttpResponseMessage outdatedAcceptance = await httpClient.PostAsJsonAsync(
            "/api/auth/me/privacy-policy/accept",
            new AcceptPrivacyPolicyRequest("outdated"));
        await ProblemDetailsAssert.HasProblemAsync(
            outdatedAcceptance,
            HttpStatusCode.BadRequest,
            "Auth.PrivacyPolicyOutdated");

        HttpResponseMessage acceptance = await httpClient.PostAsJsonAsync(
            "/api/auth/me/privacy-policy/accept",
            new AcceptPrivacyPolicyRequest(PrivacyPolicyDefaults.CurrentVersion));
        acceptance.EnsureSuccessStatusCode();

        UserProfileResponse? accepted = await acceptance.Content.ReadFromJsonAsync<UserProfileResponse>();
        Assert.NotNull(accepted);
        Assert.False(accepted.RequiresPrivacyPolicyAcceptance);
        Assert.Equal(PrivacyPolicyDefaults.CurrentVersion, accepted.PrivacyPolicyAcceptedVersion);
    }

    [Fact]
    public async Task DeleteAccountShouldEraseOwnedDataAndKeepIndependentRecipeCopies()
    {
        using HttpClient ownerHttpClient = _factory.CreateClient();
        using HttpClient readerHttpClient = _factory.CreateClient();
        var ownerClient = new ApiTestClient(ownerHttpClient);
        var readerClient = new ApiTestClient(readerHttpClient);
        string ownerEmail = TestEmail.Create("delete-owner");
        UserProfileResponse owner = await ownerClient.RegisterAsync(ownerEmail);
        await readerClient.RegisterAsync(TestEmail.Create("delete-reader"));

        RecipeResponse original = await CreateRecipeAsync(ownerHttpClient, "Удаляемый рецепт", "Public");
        RecipeResponse copy = await CopyRecipeAsync(readerHttpClient, original);
        (await readerHttpClient.PostAsync(
            new Uri($"/api/recipes/{original.Id}/favorite?revisionId={original.RevisionId}", UriKind.Relative),
            content: null)).EnsureSuccessStatusCode();

        MenuCalendarResponse? calendar = await ownerHttpClient.GetFromJsonAsync<MenuCalendarResponse>(
            "/api/menu-calendar?startDate=2026-08-24&endDate=2026-08-30");
        Assert.NotNull(calendar);
        Guid mealSlotId = Assert.Single(calendar.MealSlots, slot => slot.Name == "Ужин").Id;
        (await ownerHttpClient.PostAsJsonAsync(
            "/api/menu-calendar/items",
            new CreateMenuCalendarItemRequest(
                new DateOnly(2026, 8, 24),
                mealSlotId,
                original.Id,
                original.RevisionId,
                null,
                2,
                null))).EnsureSuccessStatusCode();
        (await ownerHttpClient.PostAsJsonAsync(
            "/api/shopping-list/items",
            new ShoppingListItemRequest(null, "Молоко", 1m, "Liter", "Dairy", null)))
            .EnsureSuccessStatusCode();

        HttpResponseMessage wrongPassword = await ownerHttpClient.PostAsJsonAsync(
            "/api/auth/me/delete",
            new DeleteAccountRequest("WrongPassword123!"));
        await ProblemDetailsAssert.HasProblemAsync(
            wrongPassword,
            HttpStatusCode.BadRequest,
            "Auth.InvalidCredentials");

        HttpResponseMessage deletion = await ownerHttpClient.PostAsJsonAsync(
            "/api/auth/me/delete",
            new DeleteAccountRequest(Password));
        Assert.Equal(HttpStatusCode.NoContent, deletion.StatusCode);
        Assert.Contains(
            deletion.Headers.GetValues("Set-Cookie"),
            value => value.StartsWith("MenuMate.RefreshToken=", StringComparison.Ordinal));

        ownerHttpClient.DefaultRequestHeaders.Authorization = null;
        HttpResponseMessage loginAfterDeletion = await ownerHttpClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(ownerEmail, Password));
        await ProblemDetailsAssert.HasProblemAsync(
            loginAfterDeletion,
            HttpStatusCode.BadRequest,
            "Auth.InvalidCredentials");

        RecipeResponse? persistedCopy = await readerHttpClient.GetFromJsonAsync<RecipeResponse>(
            $"/api/recipes/{copy.Id}");
        Assert.NotNull(persistedCopy);
        Assert.Equal(original.Id, persistedCopy.SourceRecipeId);

        HttpResponseMessage deletedOriginal = await readerHttpClient.GetAsync(
            new Uri($"/api/recipes/{original.Id}", UriKind.Relative));
        Assert.Equal(HttpStatusCode.NotFound, deletedOriginal.StatusCode);

        await AssertUserDataErasedAsync(owner.Id);
    }

    private async Task AssertUserDataErasedAsync(Guid userId)
    {
        await using AsyncServiceScope scope = _factory.Services.CreateAsyncScope();
        AuthDbContext auth = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        MenuPlanningDbContext menuPlanning = scope.ServiceProvider.GetRequiredService<MenuPlanningDbContext>();
        RecipesDbContext recipes = scope.ServiceProvider.GetRequiredService<RecipesDbContext>();
        ShoppingListsDbContext shoppingLists = scope.ServiceProvider.GetRequiredService<ShoppingListsDbContext>();

        Assert.Equal(0, await CountAuthUsersAsync(auth, userId));
        Assert.Equal(0, await CountMealSlotsAsync(menuPlanning, userId));
        Assert.Equal(0, await CountCalendarItemsAsync(menuPlanning, userId));
        Assert.Equal(0, await CountRecipesAsync(recipes, userId));
        Assert.Equal(0, await CountLibraryEntriesAsync(recipes, userId));
        Assert.Equal(0, await CountShoppingListsAsync(shoppingLists, userId));
    }

    private static Task<int> CountAuthUsersAsync(AuthDbContext dbContext, Guid userId) =>
        dbContext.Database
            .SqlQuery<int>($"SELECT COUNT(*)::int AS \"Value\" FROM auth.users WHERE id = {userId}")
            .SingleAsync();

    private static Task<int> CountMealSlotsAsync(MenuPlanningDbContext dbContext, Guid userId) =>
        dbContext.Database
            .SqlQuery<int>($"SELECT COUNT(*)::int AS \"Value\" FROM menu_planning.meal_slots WHERE owner_user_id = {userId}")
            .SingleAsync();

    private static Task<int> CountCalendarItemsAsync(MenuPlanningDbContext dbContext, Guid userId) =>
        dbContext.Database
            .SqlQuery<int>($"SELECT COUNT(*)::int AS \"Value\" FROM menu_planning.menu_calendar_items WHERE owner_user_id = {userId}")
            .SingleAsync();

    private static Task<int> CountRecipesAsync(RecipesDbContext dbContext, Guid userId) =>
        dbContext.Database
            .SqlQuery<int>($"SELECT COUNT(*)::int AS \"Value\" FROM recipes.recipes WHERE owner_user_id = {userId}")
            .SingleAsync();

    private static Task<int> CountLibraryEntriesAsync(RecipesDbContext dbContext, Guid userId) =>
        dbContext.Database
            .SqlQuery<int>($"SELECT COUNT(*)::int AS \"Value\" FROM recipes.recipe_library_entries WHERE user_id = {userId}")
            .SingleAsync();

    private static Task<int> CountShoppingListsAsync(ShoppingListsDbContext dbContext, Guid userId) =>
        dbContext.Database
            .SqlQuery<int>($"SELECT COUNT(*)::int AS \"Value\" FROM shopping_lists.shopping_lists WHERE owner_user_id = {userId}")
            .SingleAsync();

    private static async Task<RecipeResponse> CreateRecipeAsync(
        HttpClient client,
        string title,
        string visibility)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/recipes",
            CreateRecipeRequest(title, visibility));
        response.EnsureSuccessStatusCode();
        RecipeResponse? recipe = await response.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(recipe);
        return recipe;
    }

    private static async Task<RecipeResponse> CopyRecipeAsync(HttpClient client, RecipeResponse original)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/api/recipes/{original.Id}/copy",
            new CopyRecipeRequest(
                original.RevisionId,
                CreateRecipeRequest(original.Title, "Private"),
                CopySourceCover: true));
        response.EnsureSuccessStatusCode();
        RecipeResponse? copy = await response.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(copy);
        return copy;
    }

    private static CreateRecipeRequest CreateRecipeRequest(string title, string visibility) =>
        new(
            title,
            "Интеграционный тест удаления",
            2,
            "MainCourse",
            visibility,
            30,
            15,
            null,
            [new RecipeIngredientRequest(null, "Паста", 200m, "Gram", "GrainsAndPasta", null, false)],
            [new PreparationStepRequest("Приготовить")],
            []);
}
