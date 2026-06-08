using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MenuMate.Common.Presentation;

/// <summary>
/// Регистрация зависимостей presentation-слоя.
/// </summary>
public static class PresentationDependencyInjection
{
    /// <summary>
    /// Добавляет RFC 7807 ответы для необработанных ошибок API.
    /// </summary>
    public static IServiceCollection AddProblemDetailsResponses(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions["traceId"] =
                    Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;

                if (context.ProblemDetails.Status == StatusCodes.Status500InternalServerError)
                {
                    context.ProblemDetails.Title = "Server.InternalError";
                    context.ProblemDetails.Detail = "Произошла внутренняя ошибка.";
                    context.ProblemDetails.Extensions["code"] = "Server.InternalError";
                }
            };
        });

        return services;
    }

    /// <summary>
    /// Добавляет RFC 7807 ответы для middleware авторизации.
    /// </summary>
    public static IServiceCollection AddProblemDetailsAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, ProblemDetailsAuthorizationMiddlewareResultHandler>();

        return services;
    }
}
