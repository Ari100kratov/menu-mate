using MenuMate.Common.Application;
using MenuMate.Common.Presentation;
using MenuMate.Contracts.Auth;
using MenuMate.Modules.Auth.Application;
using MenuMate.Modules.Auth.Application.GetCurrentUser;
using MenuMate.Modules.Auth.Application.LoginUser;
using MenuMate.Modules.Auth.Application.LogoutUser;
using MenuMate.Modules.Auth.Application.RefreshUserToken;
using MenuMate.Modules.Auth.Application.RegisterUser;
using MenuMate.Modules.Auth.Domain.Models;
using MenuMate.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MenuMate.Modules.Auth.Presentation;

/// <summary>
/// HTTP endpoints модуля Auth.
/// </summary>
public static class AuthEndpoints
{
    private const string RefreshTokenCookieName = "MenuMate.RefreshToken";

    /// <summary>
    /// Подключает Auth endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/register", RegisterAsync)
            .WithName("RegisterUser")
            .Accepts<RegisterUserRequest>("application/json")
            .Produces<RegisterUserResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/login", LoginAsync)
            .WithName("LoginUser")
            .Accepts<LoginUserRequest>("application/json")
            .Produces<TokenResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/refresh", RefreshAsync)
            .WithName("RefreshUserToken")
            .Produces<TokenResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/me", GetCurrentUserAsync)
            .RequireAuthorization()
            .WithName("GetCurrentUser")
            .Produces<UserProfileResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("/logout", LogoutAsync)
            .RequireAuthorization()
            .WithName("LogoutUser")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return app;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterUserRequest request,
        ICommandHandler<RegisterUserCommand, RegisterUserSession> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<RegisterUserSession> result = await handler.Handle(new RegisterUserCommand(request), cancellationToken);
        if (result.IsFailure)
        {
            return result.ToHttpResult(httpContext);
        }

        SetRefreshTokenCookie(httpContext, result.Value.RefreshToken);

        var response = new RegisterUserResponse(result.Value.User, result.Value.Tokens);

        return Results.Created($"/api/auth/users/{response.User.Id}", response);
    }

    private static async Task<IResult> LoginAsync(
        LoginUserRequest request,
        ICommandHandler<LoginUserCommand, AuthSession> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<AuthSession> result = await handler.Handle(new LoginUserCommand(request), cancellationToken);
        if (result.IsFailure)
        {
            return result.ToHttpResult(httpContext);
        }

        SetRefreshTokenCookie(httpContext, result.Value.RefreshToken);

        return Results.Ok(result.Value.Tokens);
    }

    private static async Task<IResult> RefreshAsync(
        ICommandHandler<RefreshUserTokenCommand, AuthSession> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        string refreshToken = httpContext.Request.Cookies[RefreshTokenCookieName] ?? string.Empty;
        Result<AuthSession> result = await handler.Handle(new RefreshUserTokenCommand(refreshToken), cancellationToken);
        if (result.IsFailure)
        {
            ClearRefreshTokenCookie(httpContext);
            return result.ToHttpResult(httpContext);
        }

        SetRefreshTokenCookie(httpContext, result.Value.RefreshToken);

        return Results.Ok(result.Value.Tokens);
    }

    private static async Task<IResult> GetCurrentUserAsync(
        IQueryHandler<GetCurrentUserQuery, UserProfileResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<UserProfileResponse> result = await handler.Handle(new GetCurrentUserQuery(), cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> LogoutAsync(
        ICommandHandler<LogoutUserCommand> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new LogoutUserCommand(), cancellationToken);
        if (result.IsFailure)
        {
            return result.ToHttpResult(httpContext);
        }

        ClearRefreshTokenCookie(httpContext);

        return Results.NoContent();
    }

    private static void SetRefreshTokenCookie(HttpContext httpContext, RefreshToken refreshToken)
    {
        httpContext.Response.Cookies.Append(
            RefreshTokenCookieName,
            refreshToken.Value,
            CreateRefreshTokenCookieOptions(refreshToken.ExpiresAt));
    }

    private static void ClearRefreshTokenCookie(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete(
            RefreshTokenCookieName,
            CreateRefreshTokenCookieOptions(expiresAt: null));
    }

    private static CookieOptions CreateRefreshTokenCookieOptions(DateTimeOffset? expiresAt)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = expiresAt,
            Path = "/api/auth"
        };
    }
}
