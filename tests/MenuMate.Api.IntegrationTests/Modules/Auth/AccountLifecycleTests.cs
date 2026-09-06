using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MenuMate.Contracts.Auth;
using MenuMate.Modules.Auth.Application;
using MenuMate.Modules.Auth.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MenuMate.Api.IntegrationTests;

public sealed class AccountLifecycleTests : IAsyncLifetime, IDisposable
{
    private const string Password = "Password123!";
    private readonly MenuMateApiFactory _factory = new();

    public Task InitializeAsync() => _factory.InitializeAsync();

    public Task DisposeAsync() => _factory.DisposeAsync();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task FailedRegistrationEmailShouldKeepPendingAccountForResend()
    {
        string email = TestEmail.Create("delivery-failure");
        using HttpClient client = _factory.CreateClient();

        HttpResponseMessage registration = await RegisterRawAsync(client, email);
        await ProblemDetailsAssert.HasProblemAsync(
            registration,
            HttpStatusCode.BadGateway,
            "Auth.RegistrationEmailDeliveryFailed");

        HttpResponseMessage login = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(email, Password));
        await ProblemDetailsAssert.HasProblemAsync(login, HttpStatusCode.Forbidden, "Auth.EmailNotVerified");

        HttpResponseMessage resend = await client.PostAsJsonAsync(
            "/api/auth/email-verification/resend",
            new ResendEmailVerificationRequest(email));
        Assert.Equal(HttpStatusCode.Accepted, resend.StatusCode);
    }

    [Fact]
    public async Task ResendShouldInvalidatePreviousVerificationCode()
    {
        string email = TestEmail.Create("resend");
        using HttpClient client = _factory.CreateClient();
        (await RegisterRawAsync(client, email)).EnsureSuccessStatusCode();
        string oldCode = CapturingAuthEmailSender.GetVerificationCode(email);

        await MakeResendAvailableAsync(email);
        HttpResponseMessage resend = await client.PostAsJsonAsync(
            "/api/auth/email-verification/resend",
            new ResendEmailVerificationRequest(email));
        Assert.Equal(HttpStatusCode.Accepted, resend.StatusCode);
        string newCode = CapturingAuthEmailSender.GetVerificationCode(email);
        Assert.NotEqual(oldCode, newCode);

        HttpResponseMessage oldConfirmation = await client.PostAsJsonAsync(
            "/api/auth/email-verification/confirm",
            new ConfirmEmailVerificationRequest(email, oldCode));
        await ProblemDetailsAssert.HasProblemAsync(
            oldConfirmation,
            HttpStatusCode.BadRequest,
            "Auth.InvalidOrExpiredAccountAction");

        HttpResponseMessage newConfirmation = await client.PostAsJsonAsync(
            "/api/auth/email-verification/confirm",
            new ConfirmEmailVerificationRequest(email, newCode));
        newConfirmation.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task LegacyUserShouldLoginButShouldNotReceiveAutomaticPasswordReset()
    {
        string email = TestEmail.Create("legacy");
        using HttpClient client = _factory.CreateClient();
        (await RegisterRawAsync(client, email)).EnsureSuccessStatusCode();
        await MarkLegacyAsync(email);

        HttpResponseMessage login = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(email, Password));
        login.EnsureSuccessStatusCode();

        HttpResponseMessage reset = await client.PostAsJsonAsync(
            "/api/auth/password-reset/request",
            new RequestPasswordResetRequest(email));
        Assert.Equal(HttpStatusCode.Accepted, reset.StatusCode);
        Assert.False(CapturingAuthEmailSender.HasPasswordReset(email));
    }

    [Fact]
    public async Task PasswordResetShouldBeNeutralAndRevokeExistingRefreshTokens()
    {
        string email = TestEmail.Create("password-reset");
        using HttpClient client = _factory.CreateClient();
        var api = new ApiTestClient(client);
        await api.RegisterAsync(email);
        HttpResponseMessage login = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(email, Password));
        login.EnsureSuccessStatusCode();
        string refreshCookie = GetRefreshCookie(login);

        HttpResponseMessage unknown = await client.PostAsJsonAsync(
            "/api/auth/password-reset/request",
            new RequestPasswordResetRequest(TestEmail.Create("unknown")));
        Assert.Equal(HttpStatusCode.Accepted, unknown.StatusCode);

        HttpResponseMessage request = await client.PostAsJsonAsync(
            "/api/auth/password-reset/request",
            new RequestPasswordResetRequest(email));
        Assert.Equal(HttpStatusCode.Accepted, request.StatusCode);

        HttpResponseMessage complete = await client.PostAsJsonAsync(
            "/api/auth/password-reset/complete",
            new CompletePasswordResetRequest(
                CapturingAuthEmailSender.GetPasswordResetToken(email),
                "NewPassword123!"));
        complete.EnsureSuccessStatusCode();

        client.DefaultRequestHeaders.Authorization = null;
        client.DefaultRequestHeaders.Remove("Cookie");
        client.DefaultRequestHeaders.Add("Cookie", refreshCookie);
        HttpResponseMessage refresh = await client.PostAsync(
            new Uri("/api/auth/refresh", UriKind.Relative),
            content: null);
        await ProblemDetailsAssert.HasProblemAsync(
            refresh,
            HttpStatusCode.BadRequest,
            "Auth.InvalidRefreshToken");

        HttpResponseMessage oldLogin = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(email, Password));
        await ProblemDetailsAssert.HasProblemAsync(
            oldLogin,
            HttpStatusCode.BadRequest,
            "Auth.InvalidCredentials");
        (await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(email, "NewPassword123!"))).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ProfileUpdatesShouldUseIndependentSecurityRules()
    {
        string email = TestEmail.Create("profile-change");
        string newEmail = TestEmail.Create("profile-change-new");
        using HttpClient client = _factory.CreateClient();
        var api = new ApiTestClient(client);
        await api.RegisterAsync(email);

        HttpResponseMessage nameUpdate = await client.PatchAsJsonAsync(
            "/api/auth/me/display-name",
            new UpdateDisplayNameRequest("Новое имя"));
        nameUpdate.EnsureSuccessStatusCode();
        UserProfileResponse? profile = await client.GetFromJsonAsync<UserProfileResponse>("/api/auth/me");
        Assert.NotNull(profile);
        Assert.Equal("Новое имя", profile.DisplayName);

        HttpResponseMessage wrongPassword = await client.PostAsJsonAsync(
            "/api/auth/me/email-change/request",
            new RequestEmailChangeRequest(newEmail, "wrong-password"));
        await ProblemDetailsAssert.HasProblemAsync(
            wrongPassword,
            HttpStatusCode.BadRequest,
            "Auth.InvalidCredentials");

        (await client.PostAsJsonAsync(
            "/api/auth/me/email-change/request",
            new RequestEmailChangeRequest(newEmail, Password))).EnsureSuccessStatusCode();
        HttpResponseMessage confirmation = await client.PostAsJsonAsync(
            "/api/auth/me/email-change/confirm",
            new ConfirmEmailChangeRequest(CapturingAuthEmailSender.GetEmailChangeCode(newEmail)));
        confirmation.EnsureSuccessStatusCode();
        Assert.Contains(
            confirmation.Headers.GetValues("Set-Cookie"),
            value => value.Contains("MenuMate.RefreshToken=", StringComparison.Ordinal) &&
                value.Contains("expires=", StringComparison.OrdinalIgnoreCase));

        client.DefaultRequestHeaders.Authorization = null;
        HttpResponseMessage oldEmailLogin = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(email, Password));
        await ProblemDetailsAssert.HasProblemAsync(
            oldEmailLogin,
            HttpStatusCode.BadRequest,
            "Auth.InvalidCredentials");
        (await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(newEmail, Password))).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task PasswordChangeShouldRevokeRefreshTokenAndRequireNewPassword()
    {
        string email = TestEmail.Create("password-change");
        using HttpClient client = _factory.CreateClient();
        var api = new ApiTestClient(client);
        await api.RegisterAsync(email);
        HttpResponseMessage login = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(email, Password));
        login.EnsureSuccessStatusCode();
        TokenResponse? token = await login.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(token);
        string refreshCookie = GetRefreshCookie(login);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        HttpResponseMessage change = await client.PostAsJsonAsync(
            "/api/auth/me/password/change",
            new ChangePasswordRequest(Password, "ChangedPassword123!"));
        change.EnsureSuccessStatusCode();

        client.DefaultRequestHeaders.Authorization = null;
        client.DefaultRequestHeaders.Remove("Cookie");
        client.DefaultRequestHeaders.Add("Cookie", refreshCookie);
        HttpResponseMessage refresh = await client.PostAsync(
            new Uri("/api/auth/refresh", UriKind.Relative),
            content: null);
        await ProblemDetailsAssert.HasProblemAsync(
            refresh,
            HttpStatusCode.BadRequest,
            "Auth.InvalidRefreshToken");

        (await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(email, "ChangedPassword123!"))).EnsureSuccessStatusCode();
    }

    private static Task<HttpResponseMessage> RegisterRawAsync(HttpClient client, string email) =>
        client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterUserRequest(email, "Пользователь", Password, PrivacyPolicyDefaults.CurrentVersion));

    private async Task MarkLegacyAsync(string email)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        AuthDbContext dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE auth.users
            SET email_verification_status = {"LegacyUnverified"}
            WHERE email = {email};
            """);
    }

    private async Task MakeResendAvailableAsync(string email)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        AuthDbContext dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE auth.account_actions
            SET resend_available_at = NOW() - INTERVAL '1 minute'
            WHERE user_id = (SELECT id FROM auth.users WHERE email = {email});
            """);
    }

    private static string GetRefreshCookie(HttpResponseMessage response)
    {
        string setCookie = Assert.Single(
            response.Headers.GetValues("Set-Cookie"),
            value => value.StartsWith("MenuMate.RefreshToken=", StringComparison.Ordinal));
        return setCookie.Split(';', 2)[0];
    }
}
