using System.Net.Http.Headers;
using System.Net.Http.Json;
using MenuMate.Contracts.Auth;
using MenuMate.Modules.Auth.Application;

namespace MenuMate.Api.IntegrationTests;

internal sealed class ApiTestClient(HttpClient client)
{
    private const string Password = "Password123!";

    public HttpClient HttpClient => client;

    public async Task<UserProfileResponse> RegisterAsync(string email, string? displayName = null)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterUserRequest(
                email,
                displayName ?? email,
                Password,
                PrivacyPolicyDefaults.CurrentVersion));

        response.EnsureSuccessStatusCode();

        string code = CapturingAuthEmailSender.GetVerificationCode(email);
        HttpResponseMessage confirmation = await client.PostAsJsonAsync(
            "/api/auth/email-verification/confirm",
            new ConfirmEmailVerificationRequest(email, code));
        confirmation.EnsureSuccessStatusCode();

        await LoginAsync(email);
        UserProfileResponse? profile = await client.GetFromJsonAsync<UserProfileResponse>("/api/auth/me");
        Assert.NotNull(profile);
        return profile;
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
