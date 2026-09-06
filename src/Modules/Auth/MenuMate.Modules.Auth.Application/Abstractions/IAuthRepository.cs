using MenuMate.Modules.Auth.Domain.Models;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.Auth.Application.Abstractions;

internal interface IAuthRepository
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);

    Task<User?> GetUserByIdAsync(UserId userId, CancellationToken cancellationToken);

    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);

    Task<Role?> GetRoleByNameAsync(string name, CancellationToken cancellationToken);

    Task<RefreshToken?> GetRefreshTokenAsync(string value, CancellationToken cancellationToken);

    Task<AccountAction?> GetLatestAccountActionAsync(
        Guid userId,
        AccountActionPurpose purpose,
        CancellationToken cancellationToken);

    Task<AccountAction?> GetAccountActionBySecretHashAsync(
        AccountActionPurpose purpose,
        string secretHash,
        CancellationToken cancellationToken);

    Task AddUserAsync(User user, CancellationToken cancellationToken);

    Task UpdateUserAsync(User user, CancellationToken cancellationToken);

    Task DeleteUserAsync(UserId userId, CancellationToken cancellationToken);

    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken);

    Task AddAccountActionAsync(AccountAction action, CancellationToken cancellationToken);

    Task UpdateAccountActionAsync(AccountAction action, CancellationToken cancellationToken);

    Task InvalidateAccountActionsAsync(
        Guid userId,
        AccountActionPurpose purpose,
        DateTimeOffset usedAt,
        CancellationToken cancellationToken);

    Task RevokeRefreshTokenAsync(Guid refreshTokenId, CancellationToken cancellationToken);

    Task RevokeRefreshTokensForUserAsync(UserId userId, CancellationToken cancellationToken);
}
