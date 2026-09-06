using MenuMate.Common.Application;
using MenuMate.Common.Presentation;
using MenuMate.Contracts.Auth;
using MenuMate.Modules.Auth.Application;
using MenuMate.Modules.Auth.Application.AcceptPrivacyPolicy;
using MenuMate.Modules.Auth.Application.ChangePassword;
using MenuMate.Modules.Auth.Application.CompletePasswordReset;
using MenuMate.Modules.Auth.Application.ConfirmEmailChange;
using MenuMate.Modules.Auth.Application.ConfirmEmailVerification;
using MenuMate.Modules.Auth.Application.DeleteAccount;
using MenuMate.Modules.Auth.Application.GetAdminUsers;
using MenuMate.Modules.Auth.Application.GetCurrentUser;
using MenuMate.Modules.Auth.Application.GetPrivacyPolicy;
using MenuMate.Modules.Auth.Application.LoginUser;
using MenuMate.Modules.Auth.Application.LogoutUser;
using MenuMate.Modules.Auth.Application.RefreshUserToken;
using MenuMate.Modules.Auth.Application.RegisterUser;
using MenuMate.Modules.Auth.Application.RequestEmailChange;
using MenuMate.Modules.Auth.Application.RequestPasswordReset;
using MenuMate.Modules.Auth.Application.ResendEmailVerification;
using MenuMate.Modules.Auth.Application.UpdateDisplayName;
using MenuMate.Modules.Auth.Application.UpdateUserPreferences;
using MenuMate.Modules.Auth.Domain.Models;
using MenuMate.Modules.Auth.Domain.ValueObjects;
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
        app.MapGet("/api/legal/privacy-policy", GetPrivacyPolicyAsync)
            .WithTags("Legal")
            .WithName("GetPrivacyPolicy")
            .Produces<PrivacyPolicyResponse>();

        RouteGroupBuilder group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/register", RegisterAsync)
            .WithName("RegisterUser")
            .Accepts<RegisterUserRequest>("application/json")
            .Produces<RegisterUserResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status502BadGateway);

        group.MapPost("/login", LoginAsync)
            .WithName("LoginUser")
            .Accepts<LoginUserRequest>("application/json")
            .Produces<TokenResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("/email-verification/confirm", ConfirmEmailVerificationAsync)
            .WithName("ConfirmEmailVerification")
            .Accepts<ConfirmEmailVerificationRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/email-verification/resend", ResendEmailVerificationAsync)
            .WithName("ResendEmailVerification")
            .Accepts<ResendEmailVerificationRequest>("application/json")
            .Produces(StatusCodes.Status202Accepted);

        group.MapPost("/password-reset/request", RequestPasswordResetAsync)
            .WithName("RequestPasswordReset")
            .Accepts<RequestPasswordResetRequest>("application/json")
            .Produces(StatusCodes.Status202Accepted);

        group.MapPost("/password-reset/complete", CompletePasswordResetAsync)
            .WithName("CompletePasswordReset")
            .Accepts<CompletePasswordResetRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
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

        group.MapPut("/me/preferences", UpdateUserPreferencesAsync)
            .RequireAuthorization()
            .WithName("UpdateUserPreferences")
            .Accepts<UpdateUserPreferencesRequest>("application/json")
            .Produces<UserProfileResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPatch("/me/display-name", UpdateDisplayNameAsync)
            .RequireAuthorization()
            .WithName("UpdateDisplayName")
            .Accepts<UpdateDisplayNameRequest>("application/json")
            .Produces<UserProfileResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/me/email-change/request", RequestEmailChangeAsync)
            .RequireAuthorization()
            .WithName("RequestEmailChange")
            .Accepts<RequestEmailChangeRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status502BadGateway);

        group.MapPost("/me/email-change/confirm", ConfirmEmailChangeAsync)
            .RequireAuthorization()
            .WithName("ConfirmEmailChange")
            .Accepts<ConfirmEmailChangeRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/me/password/change", ChangePasswordAsync)
            .RequireAuthorization()
            .WithName("ChangePassword")
            .Accepts<ChangePasswordRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/me/privacy-policy/accept", AcceptPrivacyPolicyAsync)
            .RequireAuthorization()
            .WithName("AcceptPrivacyPolicy")
            .Accepts<AcceptPrivacyPolicyRequest>("application/json")
            .Produces<UserProfileResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/me/delete", DeleteAccountAsync)
            .RequireAuthorization()
            .WithName("DeleteAccount")
            .Accepts<DeleteAccountRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/logout", LogoutAsync)
            .RequireAuthorization()
            .WithName("LogoutUser")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        RouteGroupBuilder adminGroup = app.MapGroup("/api/admin")
            .WithTags("Administration")
            .RequireAuthorization(policy => policy.RequireRole(AuthRoleNames.Admin));

        adminGroup.MapGet("/users", GetAdminUsersAsync)
            .WithName("GetAdminUsers")
            .Produces<AdminUsersPageResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return app;
    }

    private static async Task<IResult> GetPrivacyPolicyAsync(
        IQueryHandler<GetPrivacyPolicyQuery, PrivacyPolicyResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<PrivacyPolicyResponse> result = await handler.Handle(new GetPrivacyPolicyQuery(), cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> RegisterAsync(
        RegisterUserRequest request,
        ICommandHandler<RegisterUserCommand, RegisterUserResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<RegisterUserResponse> result = await handler.Handle(new RegisterUserCommand(request), cancellationToken);
        if (result.IsFailure)
        {
            return result.ToHttpResult(httpContext);
        }

        return Results.Created("/api/auth/email-verification/confirm", result.Value);
    }

    private static async Task<IResult> ConfirmEmailVerificationAsync(
        ConfirmEmailVerificationRequest request,
        ICommandHandler<ConfirmEmailVerificationCommand> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new ConfirmEmailVerificationCommand(request), cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> ResendEmailVerificationAsync(
        ResendEmailVerificationRequest request,
        ICommandHandler<ResendEmailVerificationCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new ResendEmailVerificationCommand(request), cancellationToken);
        return Results.Accepted();
    }

    private static async Task<IResult> RequestPasswordResetAsync(
        RequestPasswordResetRequest request,
        ICommandHandler<RequestPasswordResetCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new RequestPasswordResetCommand(request), cancellationToken);
        return Results.Accepted();
    }

    private static async Task<IResult> CompletePasswordResetAsync(
        CompletePasswordResetRequest request,
        ICommandHandler<CompletePasswordResetCommand> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new CompletePasswordResetCommand(request), cancellationToken);
        if (result.IsSuccess)
        {
            ClearRefreshTokenCookie(httpContext);
        }

        return result.ToHttpResult(httpContext);
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

    private static async Task<IResult> GetAdminUsersAsync(
        string? search,
        int? page,
        int? pageSize,
        IQueryHandler<GetAdminUsersQuery, AdminUsersPageResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<AdminUsersPageResponse> result = await handler.Handle(
            new GetAdminUsersQuery(search, page ?? 1, pageSize ?? 20),
            cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> UpdateUserPreferencesAsync(
        UpdateUserPreferencesRequest request,
        ICommandHandler<UpdateUserPreferencesCommand, UserProfileResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<UserProfileResponse> result = await handler.Handle(
            new UpdateUserPreferencesCommand(request),
            cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> UpdateDisplayNameAsync(
        UpdateDisplayNameRequest request,
        ICommandHandler<UpdateDisplayNameCommand, UserProfileResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<UserProfileResponse> result = await handler.Handle(
            new UpdateDisplayNameCommand(request),
            cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> RequestEmailChangeAsync(
        RequestEmailChangeRequest request,
        ICommandHandler<RequestEmailChangeCommand> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new RequestEmailChangeCommand(request), cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> ConfirmEmailChangeAsync(
        ConfirmEmailChangeRequest request,
        ICommandHandler<ConfirmEmailChangeCommand> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new ConfirmEmailChangeCommand(request), cancellationToken);
        if (result.IsSuccess)
        {
            ClearRefreshTokenCookie(httpContext);
        }

        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> ChangePasswordAsync(
        ChangePasswordRequest request,
        ICommandHandler<ChangePasswordCommand> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new ChangePasswordCommand(request), cancellationToken);
        if (result.IsSuccess)
        {
            ClearRefreshTokenCookie(httpContext);
        }

        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> AcceptPrivacyPolicyAsync(
        AcceptPrivacyPolicyRequest request,
        ICommandHandler<AcceptPrivacyPolicyCommand, UserProfileResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<UserProfileResponse> result = await handler.Handle(
            new AcceptPrivacyPolicyCommand(request),
            cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> DeleteAccountAsync(
        DeleteAccountRequest request,
        ICommandHandler<DeleteAccountCommand> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new DeleteAccountCommand(request), cancellationToken);
        if (result.IsSuccess)
        {
            ClearRefreshTokenCookie(httpContext);
        }

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
