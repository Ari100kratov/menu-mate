using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MenuMate.Common.Presentation;

/// <summary>
/// Преобразует ответы авторизации в формат проблем RFC 7807.
/// </summary>
public sealed class ProblemDetailsAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    /// <inheritdoc />
    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(authorizeResult);

        if (!authorizeResult.Challenged && !authorizeResult.Forbidden)
        {
            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult).ConfigureAwait(false);
            return;
        }

        int statusCode = authorizeResult.Challenged
            ? StatusCodes.Status401Unauthorized
            : StatusCodes.Status403Forbidden;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = authorizeResult.Challenged ? "Auth.Unauthorized" : "Auth.Forbidden",
            Detail = authorizeResult.Challenged
                ? "Требуется аутентификация."
                : "Текущий пользователь не может получить доступ к ресурсу.",
            Instance = context.Request.Path
        };

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = statusCode;

        await context.Response
            .WriteAsJsonAsync(problemDetails, options: null, contentType: "application/problem+json")
            .ConfigureAwait(false);
    }
}
