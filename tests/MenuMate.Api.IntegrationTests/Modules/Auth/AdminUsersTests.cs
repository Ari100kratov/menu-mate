using System.Net;
using System.Net.Http.Json;
using MenuMate.Contracts.Auth;
using MenuMate.Contracts.Recipes;
using MenuMate.Modules.Auth.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MenuMate.Api.IntegrationTests;

public sealed class AdminUsersTests : IAsyncLifetime, IDisposable
{
    private readonly MenuMateApiFactory _factory = new();

    public Task InitializeAsync() => _factory.InitializeAsync();

    public Task DisposeAsync() => _factory.DisposeAsync();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task UsersEndpointShouldRejectRegularUser()
    {
        using HttpClient httpClient = _factory.CreateClient();
        var client = new ApiTestClient(httpClient);

        await client.RegisterAsync(TestEmail.Create("admin-users-forbidden"));

        HttpResponseMessage response = await httpClient.GetAsync(new Uri("/api/admin/users", UriKind.Relative));

        await ProblemDetailsAssert.HasProblemAsync(response, HttpStatusCode.Forbidden, "Auth.Forbidden");
    }

    [Fact]
    public async Task AdminShouldSearchAndBrowseRegisteredUsers()
    {
        using HttpClient adminHttpClient = _factory.CreateClient();
        using HttpClient targetHttpClient = _factory.CreateClient();
        using HttpClient otherHttpClient = _factory.CreateClient();
        var adminClient = new ApiTestClient(adminHttpClient);
        var targetClient = new ApiTestClient(targetHttpClient);
        var otherClient = new ApiTestClient(otherHttpClient);

        RegisterUserResponse target = await targetClient.RegisterAsync(TestEmail.Create("admin-users-target"));
        await CreateRecipeAsync(targetHttpClient);
        await otherClient.RegisterAsync(TestEmail.Create("admin-users-other"));
        RegisterUserResponse admin = await adminClient.RegisterAsync(TestEmail.Create("admin-users-admin"));
        await PromoteToAdminAsync(admin.User.Id);
        await adminClient.LoginAsync(admin.User.Email);

        AdminUsersPageResponse? searched = await adminHttpClient.GetFromJsonAsync<AdminUsersPageResponse>(
            $"/api/admin/users?search={Uri.EscapeDataString(target.User.Email)}&page=1&pageSize=20");
        AdminUsersPageResponse? firstPage = await adminHttpClient.GetFromJsonAsync<AdminUsersPageResponse>(
            "/api/admin/users?page=1&pageSize=1");

        Assert.NotNull(searched);
        Assert.NotNull(firstPage);
        AdminUserListItemResponse user = Assert.Single(searched.Items);
        Assert.Equal(target.User.Id, user.Id);
        Assert.Equal(target.User.Email, user.Email);
        Assert.Equal(target.User.DisplayName, user.DisplayName);
        Assert.Contains("user", user.Roles);
        Assert.Equal(1, user.RecipesCount);
        Assert.NotEqual(default, user.RegisteredAt);
        Assert.Equal(1, searched.TotalCount);
        Assert.Equal(1, searched.Page);
        Assert.Equal(20, searched.PageSize);
        Assert.Single(firstPage.Items);
        Assert.True(firstPage.TotalCount >= 3);
        Assert.Equal(1, firstPage.Page);
        Assert.Equal(1, firstPage.PageSize);
    }

    private async Task PromoteToAdminAsync(Guid userId)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        AuthDbContext dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO auth.user_roles (user_id, role_id)
            SELECT {userId}, id
            FROM auth.roles
            WHERE name = {"admin"}
            ON CONFLICT (user_id, role_id) DO NOTHING;
            """);
    }

    private static async Task CreateRecipeAsync(HttpClient httpClient)
    {
        var request = new CreateRecipeRequest(
            "Тестовый рецепт",
            null,
            2,
            "MainCourse",
            "Private",
            null,
            null,
            null,
            [new RecipeIngredientRequest(null, "Макароны", 200m, "Gram", "GrainsAndPasta", null, false)],
            [new PreparationStepRequest("Приготовить")],
            []);

        HttpResponseMessage response = await httpClient.PostAsJsonAsync("/api/recipes", request);
        response.EnsureSuccessStatusCode();
    }
}
