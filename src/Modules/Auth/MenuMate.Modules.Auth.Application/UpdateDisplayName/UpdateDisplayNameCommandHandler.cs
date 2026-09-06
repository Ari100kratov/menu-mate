using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;
using MenuMate.Modules.Auth.Application.Abstractions;
using MenuMate.Modules.Auth.Domain.Errors;
using MenuMate.Modules.Auth.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Auth.Application.UpdateDisplayName;

internal sealed class UpdateDisplayNameCommandHandler(
    IAuthRepository repository,
    IAuthUnitOfWork unitOfWork,
    IUserContext userContext,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateDisplayNameCommand, UserProfileResponse>
{
    public async Task<Result<UserProfileResponse>> Handle(
        UpdateDisplayNameCommand command,
        CancellationToken cancellationToken)
    {
        User? user = await repository.GetUserByIdAsync(userContext.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserProfileResponse>(AuthErrors.UserNotFound(userContext.UserId));
        }

        Result result = user.UpdateDisplayName(command.Request.DisplayName, timeProvider.GetUtcNow());
        if (result.IsFailure)
        {
            return Result.Failure<UserProfileResponse>(result.Error);
        }

        await repository.UpdateUserAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return AuthMapping.ToProfile(user);
    }
}
