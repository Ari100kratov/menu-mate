using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;
using MenuMate.Modules.Auth.Application.GetAdminUsers;
using MenuMate.Modules.Auth.Application.ChangePassword;
using MenuMate.Modules.Auth.Application.AcceptPrivacyPolicy;
using MenuMate.Modules.Auth.Application.CompletePasswordReset;
using MenuMate.Modules.Auth.Application.ConfirmEmailChange;
using MenuMate.Modules.Auth.Application.ConfirmEmailVerification;
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
using MenuMate.Modules.Auth.Application.DeleteAccount;
using MenuMate.Modules.Auth.Application.UpdateUserPreferences;
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
        services.AddScoped<ICommandHandler<RegisterUserCommand, RegisterUserResponse>, RegisterUserCommandHandler>();
        services.AddScoped<ICommandHandler<ConfirmEmailVerificationCommand>, ConfirmEmailVerificationCommandHandler>();
        services.AddScoped<ICommandHandler<ResendEmailVerificationCommand>, ResendEmailVerificationCommandHandler>();
        services.AddScoped<ICommandHandler<RequestPasswordResetCommand>, RequestPasswordResetCommandHandler>();
        services.AddScoped<ICommandHandler<CompletePasswordResetCommand>, CompletePasswordResetCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateDisplayNameCommand, UserProfileResponse>, UpdateDisplayNameCommandHandler>();
        services.AddScoped<ICommandHandler<RequestEmailChangeCommand>, RequestEmailChangeCommandHandler>();
        services.AddScoped<ICommandHandler<ConfirmEmailChangeCommand>, ConfirmEmailChangeCommandHandler>();
        services.AddScoped<ICommandHandler<ChangePasswordCommand>, ChangePasswordCommandHandler>();
        services.AddScoped<ICommandHandler<AcceptPrivacyPolicyCommand, UserProfileResponse>, AcceptPrivacyPolicyCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteAccountCommand>, DeleteAccountCommandHandler>();
        services.AddScoped<IQueryHandler<GetPrivacyPolicyQuery, PrivacyPolicyResponse>, GetPrivacyPolicyQueryHandler>();
        services.AddScoped<ICommandHandler<LoginUserCommand, AuthSession>, LoginUserCommandHandler>();
        services.AddScoped<ICommandHandler<RefreshUserTokenCommand, AuthSession>, RefreshUserTokenCommandHandler>();
        services.AddScoped<ICommandHandler<LogoutUserCommand>, LogoutUserCommandHandler>();
        services.AddScoped<IQueryHandler<GetCurrentUserQuery, UserProfileResponse>, GetCurrentUserQueryHandler>();
        services.AddScoped<IQueryHandler<GetAdminUsersQuery, AdminUsersPageResponse>, GetAdminUsersQueryHandler>();
        services.AddScoped<ICommandHandler<UpdateUserPreferencesCommand, UserProfileResponse>, UpdateUserPreferencesCommandHandler>();

        return services;
    }
}
