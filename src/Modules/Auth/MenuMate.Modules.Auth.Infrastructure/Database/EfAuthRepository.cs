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

    public async Task AddUserAsync(User user, CancellationToken cancellationToken)
    {
        await dbContext.Users.AddAsync(UserRecord.FromDomain(user), cancellationToken);
    }

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        await dbContext.RefreshTokens.AddAsync(RefreshTokenRecord.FromDomain(refreshToken), cancellationToken);
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
