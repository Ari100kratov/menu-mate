using System.Text;
using MenuMate.Common.Application;
using MenuMate.Modules.Auth.Application.Abstractions;
using MenuMate.Modules.Auth.Infrastructure.Authentication;
using MenuMate.Modules.Auth.Infrastructure.Database;
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
        services.AddScoped<IUserContext, UserContext>();

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
}
