using System.Net;
using System.Net.Http.Json;
using MenuMate.Contracts.Auth;

namespace MenuMate.Api.IntegrationTests;

public sealed class AuthValidationTests : IAsyncLifetime, IDisposable
{
    private readonly MenuMateApiFactory _factory = new();

    public Task InitializeAsync() => _factory.InitializeAsync();

    public Task DisposeAsync() => _factory.DisposeAsync();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task RegisteringDuplicateEmailShouldReturnConflict()
    {
        using HttpClient httpClient = _factory.CreateClient();
        var client = new ApiTestClient(httpClient);
        string email = TestEmail.Create("duplicate");
        await client.RegisterAsync(email);

        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterUserRequest(email.ToUpperInvariant(), "Другой пользователь", "Password123!"));

        await ProblemDetailsAssert.HasProblemAsync(response, HttpStatusCode.Conflict, "Auth.EmailNotUnique");
    }

    [Fact]
    public async Task LoginWithWrongPasswordShouldReturnValidationProblem()
    {
        using HttpClient httpClient = _factory.CreateClient();
        var client = new ApiTestClient(httpClient);
        string email = TestEmail.Create("wrong-password");
        await client.RegisterAsync(email);

        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(email, "WrongPassword123!"));

        await ProblemDetailsAssert.HasProblemAsync(response, HttpStatusCode.BadRequest, "Auth.InvalidCredentials");
    }
}
