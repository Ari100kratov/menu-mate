using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;
using MenuMate.Modules.Auth.Application.GetAdminUsers;
using MenuMate.Modules.Auth.Application.GetCurrentUser;
using MenuMate.Modules.Auth.Application.LoginUser;
using MenuMate.Modules.Auth.Application.LogoutUser;
using MenuMate.Modules.Auth.Application.RefreshUserToken;
using MenuMate.Modules.Auth.Application.RegisterUser;
using Microsoft.Extensions.DependencyInjection;

namespace MenuMate.Modules.Auth.Application;

/// <summary>
/// Регистрация прикладного слоя Auth.
/// </summary>
public static class AuthApplicationDependencyInjection
{
    /// <summary>
    /// Добавляет обработчики сценариев Auth.
    /// </summary>
    public static IServiceCollection AddAuthApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<RegisterUserCommand, RegisterUserSession>, RegisterUserCommandHandler>();
        services.AddScoped<ICommandHandler<LoginUserCommand, AuthSession>, LoginUserCommandHandler>();
        services.AddScoped<ICommandHandler<RefreshUserTokenCommand, AuthSession>, RefreshUserTokenCommandHandler>();
        services.AddScoped<ICommandHandler<LogoutUserCommand>, LogoutUserCommandHandler>();
        services.AddScoped<IQueryHandler<GetCurrentUserQuery, UserProfileResponse>, GetCurrentUserQueryHandler>();
        services.AddScoped<IQueryHandler<GetAdminUsersQuery, AdminUsersPageResponse>, GetAdminUsersQueryHandler>();

        return services;
    }
}
