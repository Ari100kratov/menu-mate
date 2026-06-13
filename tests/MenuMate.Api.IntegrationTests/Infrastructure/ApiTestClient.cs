using System.Net.Http.Headers;
using System.Net.Http.Json;
using MenuMate.Contracts.Auth;

namespace MenuMate.Api.IntegrationTests;

internal sealed class ApiTestClient(HttpClient client)
{
    private const string Password = "Password123!";

    public HttpClient HttpClient => client;

    public async Task<RegisterUserResponse> RegisterAsync(string email)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterUserRequest(email, email, Password));

        response.EnsureSuccessStatusCode();

        RegisterUserResponse? content = await response.Content.ReadFromJsonAsync<RegisterUserResponse>();
        Assert.NotNull(content);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            content.Tokens.AccessToken);

        return content;
    }

    public async Task<TokenResponse> LoginAsync(string email)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(email, Password));

        response.EnsureSuccessStatusCode();

        TokenResponse? content = await response.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(content);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", content.AccessToken);
        return content;
    }
}
