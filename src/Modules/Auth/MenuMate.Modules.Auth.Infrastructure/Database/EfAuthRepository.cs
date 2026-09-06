using MenuMate.Modules.Auth.Application.Abstractions;
using MenuMate.Modules.Auth.Domain.Models;
using MenuMate.Modules.Auth.Infrastructure.Database.Entities;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace MenuMate.Modules.Auth.Infrastructure.Database;

internal sealed class EfAuthRepository(AuthDbContext dbContext) : IAuthRepository
{
    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) =>
        dbContext.Users.AnyAsync(user => user.Email == email, cancellationToken);

    public async Task<User?> GetUserByIdAsync(UserId userId, CancellationToken cancellationToken)
    {
        UserRecord? record = await UserQuery()
            .SingleOrDefaultAsync(user => user.Id == userId.Value, cancellationToken);

        return record?.ToDomain();
    }

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        UserRecord? record = await UserQuery()
            .SingleOrDefaultAsync(user => user.Email == email, cancellationToken);

        return record?.ToDomain();
    }

    public async Task<Role?> GetRoleByNameAsync(string name, CancellationToken cancellationToken)
    {
        RoleRecord? record = await dbContext.Roles
            .SingleOrDefaultAsync(role => role.Name == name, cancellationToken);

        return record?.ToDomain();
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string value, CancellationToken cancellationToken)
    {
        RefreshTokenRecord? record = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(token => token.Value == value, cancellationToken);

        return record?.ToDomain();
    }

    public async Task<AccountAction?> GetLatestAccountActionAsync(
        Guid userId,
        AccountActionPurpose purpose,
        CancellationToken cancellationToken)
    {
        AccountActionRecord? record = await dbContext.AccountActions
            .Where(action => action.UserId == userId && action.Purpose == purpose)
            .OrderByDescending(action => action.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return record?.ToDomain();
    }

    public async Task<AccountAction?> GetAccountActionBySecretHashAsync(
        AccountActionPurpose purpose,
        string secretHash,
        CancellationToken cancellationToken)
    {
        AccountActionRecord? record = await dbContext.AccountActions
            .SingleOrDefaultAsync(
                action => action.Purpose == purpose && action.SecretHash == secretHash,
                cancellationToken);

        return record?.ToDomain();
    }

    public async Task AddUserAsync(User user, CancellationToken cancellationToken)
    {
        await dbContext.Users.AddAsync(UserRecord.FromDomain(user), cancellationToken);
    }

    public async Task UpdateUserAsync(User user, CancellationToken cancellationToken)
    {
        UserRecord? record = await dbContext.Users
            .SingleOrDefaultAsync(existing => existing.Id == user.Id, cancellationToken);

        record?.Apply(user);
    }

    public async Task DeleteUserAsync(UserId userId, CancellationToken cancellationToken)
    {
        UserRecord? record = await dbContext.Users
            .SingleOrDefaultAsync(user => user.Id == userId.Value, cancellationToken);
        if (record is not null)
        {
            dbContext.Users.Remove(record);
        }
    }

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        await dbContext.RefreshTokens.AddAsync(RefreshTokenRecord.FromDomain(refreshToken), cancellationToken);
    }

    public async Task AddAccountActionAsync(AccountAction action, CancellationToken cancellationToken) =>
        await dbContext.AccountActions.AddAsync(AccountActionRecord.FromDomain(action), cancellationToken);

    public async Task UpdateAccountActionAsync(AccountAction action, CancellationToken cancellationToken)
    {
        AccountActionRecord? record = await dbContext.AccountActions
            .SingleOrDefaultAsync(existing => existing.Id == action.Id, cancellationToken);
        record?.Apply(action);
    }

    public async Task InvalidateAccountActionsAsync(
        Guid userId,
        AccountActionPurpose purpose,
        DateTimeOffset usedAt,
        CancellationToken cancellationToken)
    {
        List<AccountActionRecord> records = await dbContext.AccountActions
            .Where(action => action.UserId == userId && action.Purpose == purpose && action.UsedAt == null)
            .ToListAsync(cancellationToken);

        foreach (AccountActionRecord record in records)
        {
            record.UsedAt = usedAt;
        }
    }

    public async Task RevokeRefreshTokenAsync(Guid refreshTokenId, CancellationToken cancellationToken)
    {
        RefreshTokenRecord? record = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(token => token.Id == refreshTokenId, cancellationToken);

        record?.IsRevoked = true;
    }

    public async Task RevokeRefreshTokensForUserAsync(UserId userId, CancellationToken cancellationToken)
    {
        List<RefreshTokenRecord> records = await dbContext.RefreshTokens
            .Where(token => token.UserId == userId.Value && !token.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (RefreshTokenRecord record in records)
        {
            record.IsRevoked = true;
        }
    }

    private IQueryable<UserRecord> UserQuery() =>
        dbContext.Users
            .Include(user => user.Roles)
            .ThenInclude(role => role.Role)
            .Include(user => user.RefreshTokens);
}
