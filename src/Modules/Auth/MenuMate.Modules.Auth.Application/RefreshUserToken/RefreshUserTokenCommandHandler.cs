using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;
using MenuMate.Modules.Auth.Application;
using MenuMate.Modules.Auth.Application.Abstractions;
using MenuMate.Modules.Auth.Domain.Errors;
using MenuMate.Modules.Auth.Domain.Models;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.Auth.Application.RefreshUserToken;

internal sealed class RefreshUserTokenCommandHandler(
    IAuthRepository repository,
    IAuthUnitOfWork unitOfWork,
    ITokenProvider tokenProvider,
    IRefreshTokenService refreshTokenService)
    : ICommandHandler<RefreshUserTokenCommand, AuthSession>
{
    public async Task<Result<AuthSession>> Handle(
        RefreshUserTokenCommand command,
        CancellationToken cancellationToken)
    {
        RefreshToken? token = await repository.GetRefreshTokenAsync(command.RefreshToken, cancellationToken);
        if (token is null || !refreshTokenService.IsValid(token))
        {
            return Result.Failure<AuthSession>(AuthErrors.InvalidRefreshToken);
        }

        User? user = await repository.GetUserByIdAsync(UserId.From(token.UserId), cancellationToken);
        if (user is null)
        {
            return Result.Failure<AuthSession>(AuthErrors.InvalidRefreshToken);
        }

        await repository.RevokeRefreshTokenAsync(token.Id, cancellationToken);

        AccessToken accessToken = tokenProvider.Create(user);
        RefreshToken refreshToken = refreshTokenService.Generate(user);

        await repository.AddRefreshTokenAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthSession(AuthMapping.ToTokenResponse(accessToken), refreshToken);
    }
}
