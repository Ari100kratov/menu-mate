using System.Text;
using System.Net.Mail;
using MenuMate.Common.Application;
using MenuMate.Modules.Auth.Application.Abstractions;
using MenuMate.Modules.Auth.Infrastructure.Authentication;
using MenuMate.Modules.Auth.Infrastructure.Database;
using MenuMate.Modules.Auth.Infrastructure.Email;
using MenuMate.Modules.Auth.Infrastructure.Legal;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace MenuMate.Modules.Auth.Infrastructure;

/// <summary>
/// Регистрация инфраструктуры Auth.
/// </summary>
public static class AuthInfrastructureDependencyInjection
{
    /// <summary>
    /// Добавляет сервисы сохранения данных и аутентификации Auth.
    /// </summary>
    public static IServiceCollection AddAuthInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        string connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' is not configured.");

        services.AddDbContext<AuthDbContext>(options =>
        {
            options.UseNpgsql(
                    connectionString,
                    npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                        HistoryRepository.DefaultTableName,
                        AuthSchema.Name))
                .UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IAuthRepository, EfAuthRepository>();
        services.AddScoped<IAuthUnitOfWork>(provider => provider.GetRequiredService<AuthDbContext>());
        services.AddScoped<IAuthReadDbContext>(provider => provider.GetRequiredService<AuthDbContext>());

        services.AddHttpContextAccessor();
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenProvider, TokenProvider>();
        services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
        services.AddSingleton<IAccountActionTokenService, AccountActionTokenService>();
        services.AddSingleton<IAuthEmailSender, SmtpAuthEmailSender>();
        services.AddSingleton<IAuthUrlBuilder, AuthUrlBuilder>();
        services.AddSingleton<IPrivacyPolicyProvider, PrivacyPolicyProvider>();
        services.AddScoped<IUserContext, UserContext>();

        services.AddOptions<AccountActionOptions>()
            .Bind(configuration.GetSection(AccountActionOptions.SectionName))
            .Validate(
                options => options.HashSecret.Length >= 32,
                "AccountActions:HashSecret must contain at least 32 characters.")
            .ValidateOnStart();

        services.AddOptions<EmailOptions>()
            .Bind(configuration.GetSection(EmailOptions.SectionName))
            .Validate(
                IsEmailConfigurationValid,
                "Enabled email delivery requires a valid SMTP host, port, security mode and FromAddress.")
            .ValidateOnStart();

        services.AddOptions<PublicWebOptions>()
            .Bind(configuration.GetSection(PublicWebOptions.SectionName))
            .Validate(
                options => !configuration.GetValue<bool>("Email:Enabled") ||
                    IsPublicWebUrlValid(options.BaseUrl),
                "Enabled email delivery requires an HTTPS PublicWeb:BaseUrl (HTTP is allowed only for loopback development).")
            .ValidateOnStart();

        services.AddOptions<LegalOptions>()
            .Bind(configuration.GetSection(LegalOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.OperatorName) &&
                    MailAddress.TryCreate(options.PrivacyContactEmail, out _) &&
                    options.TechnicalLogRetentionDays > 0 &&
                    options.BackupRetentionDays > 0,
                "Legal settings require an operator, privacy contact and positive retention periods.")
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Secret"] ?? AuthDefaults.JwtSecret)),
                    ValidIssuer = configuration["Jwt:Issuer"] ?? AuthDefaults.JwtIssuer,
                    ValidAudience = configuration["Jwt:Audience"] ?? AuthDefaults.JwtAudience,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }

    private static bool IsEmailConfigurationValid(EmailOptions options)
    {
        if (!options.Enabled)
        {
            return true;
        }

        bool validSecurity = options.Smtp.Security.Equals("None", StringComparison.OrdinalIgnoreCase) ||
            options.Smtp.Security.Equals("StartTls", StringComparison.OrdinalIgnoreCase) ||
            options.Smtp.Security.Equals("SslOnConnect", StringComparison.OrdinalIgnoreCase);

        return !string.IsNullOrWhiteSpace(options.Smtp.Host) &&
            options.Smtp.Port is > 0 and <= 65535 &&
            validSecurity &&
            MailAddress.TryCreate(options.FromAddress, out _);
    }

    private static bool IsPublicWebUrlValid(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out Uri? uri))
        {
            return false;
        }

        return uri.Scheme == Uri.UriSchemeHttps ||
            (uri.IsLoopback && uri.Scheme == Uri.UriSchemeHttp);
    }
}
