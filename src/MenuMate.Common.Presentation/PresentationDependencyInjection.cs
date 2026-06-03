using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace MenuMate.Common.Presentation;

/// <summary>
/// Регистрация зависимостей presentation-слоя.
/// </summary>
public static class PresentationDependencyInjection
{
    /// <summary>
    /// Добавляет RFC 7807 ответы для middleware авторизации.
    /// </summary>
    public static IServiceCollection AddProblemDetailsAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, ProblemDetailsAuthorizationMiddlewareResultHandler>();

        return services;
    }
}
