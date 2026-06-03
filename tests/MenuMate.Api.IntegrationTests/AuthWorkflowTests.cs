using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MenuMate.Contracts.Auth;

namespace MenuMate.Api.IntegrationTests;

public sealed class AuthWorkflowTests : IAsyncLifetime, IDisposable
{
    private readonly MenuMateApiFactory _factory = new();

    public Task InitializeAsync() => _factory.InitializeAsync();

    public Task DisposeAsync() => _factory.DisposeAsync();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task ProtectedEndpointWithoutTokenShouldReturnProblemDetails()
    {
        using HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync(new Uri("/api/recipes/", UriKind.Relative));

        await ProblemDetailsAssert.HasProblemAsync(response, HttpStatusCode.Unauthorized, "Auth.Unauthorized");
    }

    [Fact]
    public async Task RegisterLoginAndMeShouldReturnCurrentUser()
    {
        string email = TestEmail.Create("auth");
        using HttpClient httpClient = _factory.CreateClient();
        var client = new ApiTestClient(httpClient);

        RegisterUserResponse registered = await client.RegisterAsync(email);
        TokenResponse tokens = await client.LoginAsync(email);
        UserProfileResponse? me = await httpClient.GetFromJsonAsync<UserProfileResponse>("/api/auth/me");

        Assert.Equal(email, registered.User.Email);
        Assert.False(string.IsNullOrWhiteSpace(tokens.AccessToken));
        Assert.NotNull(me);
        Assert.Equal(registered.User.Id, me.Id);
        Assert.Contains("user", me.Roles);
    }

    [Fact]
    public async Task RefreshTokenShouldBeStoredInHttpOnlyCookieAndRotated()
    {
        string email = TestEmail.Create("auth-cookie");
        using HttpClient httpClient = _factory.CreateClient();

        HttpResponseMessage registerResponse = await httpClient.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterUserRequest(email, email, "Password123!"));

        registerResponse.EnsureSuccessStatusCode();

        string refreshCookie = GetRefreshCookie(registerResponse);
        JsonElement registerJson = await registerResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.True(registerJson.TryGetProperty("tokens", out JsonElement tokensJson));
        Assert.False(tokensJson.TryGetProperty("refreshToken", out _));

        TokenResponse? registerTokens = tokensJson.Deserialize<TokenResponse>(JsonSerializerOptions.Web);
        Assert.NotNull(registerTokens);

        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            registerTokens.AccessToken);
        httpClient.DefaultRequestHeaders.Add("Cookie", refreshCookie);

        HttpResponseMessage refreshResponse = await httpClient.PostAsync(
            new Uri("/api/auth/refresh", UriKind.Relative),
            content: null);

        refreshResponse.EnsureSuccessStatusCode();

        string rotatedRefreshCookie = GetRefreshCookie(refreshResponse);
        TokenResponse? refreshTokens = await refreshResponse.Content.ReadFromJsonAsync<TokenResponse>();

        Assert.NotNull(refreshTokens);
        Assert.False(string.IsNullOrWhiteSpace(refreshTokens.AccessToken));
        Assert.StartsWith("MenuMate.RefreshToken=", rotatedRefreshCookie, StringComparison.Ordinal);

        httpClient.DefaultRequestHeaders.Remove("Cookie");
        httpClient.DefaultRequestHeaders.Add("Cookie", rotatedRefreshCookie);

        HttpResponseMessage logoutResponse = await httpClient.PostAsync(
            new Uri("/api/auth/logout", UriKind.Relative),
            content: null);

        logoutResponse.EnsureSuccessStatusCode();

        Assert.Contains(
            logoutResponse.Headers.GetValues("Set-Cookie"),
            value => value.Contains("MenuMate.RefreshToken=", StringComparison.Ordinal)
                && value.Contains("expires=", StringComparison.OrdinalIgnoreCase));
    }

    private static string GetRefreshCookie(HttpResponseMessage response)
    {
        string setCookie = Assert.Single(
            response.Headers.GetValues("Set-Cookie"),
            value => value.StartsWith("MenuMate.RefreshToken=", StringComparison.Ordinal));

        Assert.Contains("httponly", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", setCookie, StringComparison.OrdinalIgnoreCase);

        return setCookie.Split(';', 2)[0];
    }
}
