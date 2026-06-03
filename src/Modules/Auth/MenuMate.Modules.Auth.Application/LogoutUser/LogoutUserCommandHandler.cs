using MenuMate.Common.Application;
using MenuMate.Modules.Auth.Application.Abstractions;
using MenuMate.Modules.Auth.Domain.Errors;
using MenuMate.Modules.Auth.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Auth.Application.LogoutUser;

internal sealed class LogoutUserCommandHandler(
    IAuthRepository repository,
    IAuthUnitOfWork unitOfWork,
    IUserContext userContext)
    : ICommandHandler<LogoutUserCommand>
{
    public async Task<Result> Handle(LogoutUserCommand command, CancellationToken cancellationToken)
    {
        User? user = await repository.GetUserByIdAsync(userContext.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(AuthErrors.UserNotFound(userContext.UserId));
        }

        await repository.RevokeRefreshTokensForUserAsync(userContext.UserId, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
