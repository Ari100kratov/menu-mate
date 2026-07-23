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
    async Task<AdminUsersPageReadModel> IAuthReadDbContext.GetAdminUsersAsync(
        string? search,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        string? normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        IQueryable<UserRecord> query = Users.AsNoTracking();

        if (normalizedSearch is not null)
        {
            string pattern = $"%{normalizedSearch}%";
            query = query.Where(user =>
                EF.Functions.ILike(user.Email, pattern) ||
                EF.Functions.ILike(user.DisplayName, pattern));
        }

        int totalCount = await query.CountAsync(cancellationToken);
        AdminUserReadModel[] users = await query
            .OrderByDescending(user => user.CreatedAt)
            .ThenBy(user => user.Id)
            .Skip(skip)
            .Take(take)
            .Select(user => new AdminUserReadModel(
                user.Id,
                user.Email,
                user.DisplayName,
                user.CreatedAt,
                user.Roles
                    .OrderBy(role => role.Role!.Name)
                    .Select(role => role.Role!.Name)
                    .ToArray()))
            .ToArrayAsync(cancellationToken);

        return new AdminUsersPageReadModel(totalCount, users);
    }

    /// <summary>
    /// Возвращает идентификатор существующего администратора по адресу электронной почты.
    /// </summary>
    public async Task<UserId?> FindAdminUserIdByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        string normalizedEmail = Domain.ValueObjects.EmailNormalizer.Normalize(email);
        Guid? userId = await Users
            .AsNoTracking()
            .Where(user =>
                user.Email == normalizedEmail &&
                user.Roles.Any(role => role.Role!.Name == Domain.ValueObjects.AuthRoleNames.Admin))
            .Select(user => (Guid?)user.Id)
            .SingleOrDefaultAsync(cancellationToken);

        return userId.HasValue ? UserId.From(userId.Value) : null;
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(AuthSchema.Name);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
    }
}
