using MenuMate.Contracts.Auth;
using MenuMate.Modules.Auth.Application.Abstractions;
using MenuMate.Modules.Auth.Infrastructure.Database.Entities;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace MenuMate.Modules.Auth.Infrastructure.Database;

/// <summary>
/// EF Core DbContext модуля Auth.
/// </summary>
public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options)
    : DbContext(options), IAuthUnitOfWork, IAuthReadDbContext
{
    internal DbSet<UserRecord> Users => Set<UserRecord>();

    internal DbSet<RoleRecord> Roles => Set<RoleRecord>();

    internal DbSet<UserRoleRecord> UserRoles => Set<UserRoleRecord>();

    internal DbSet<RefreshTokenRecord> RefreshTokens => Set<RefreshTokenRecord>();

    /// <inheritdoc />
    public async Task<UserProfileResponse?> GetUserProfileAsync(
        UserId userId,
        CancellationToken cancellationToken)
    {
        return await Users
            .AsNoTracking()
            .Where(user => user.Id == userId.Value)
            .Select(user => new UserProfileResponse(
                user.Id,
                user.Email,
                user.DisplayName,
                user.Roles
                    .OrderBy(role => role.Role!.Name)
                    .Select(role => role.Role!.Name)
                    .ToArray()))
            .SingleOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(AuthSchema.Name);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
    }
}
