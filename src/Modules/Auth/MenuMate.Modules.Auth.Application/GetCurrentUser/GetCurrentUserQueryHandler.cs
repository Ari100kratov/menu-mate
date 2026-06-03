using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;
using MenuMate.Modules.Auth.Application.Abstractions;
using MenuMate.Modules.Auth.Domain.Errors;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Auth.Application.GetCurrentUser;

internal sealed class GetCurrentUserQueryHandler(
    IAuthReadDbContext dbContext,
    IUserContext userContext)
    : IQueryHandler<GetCurrentUserQuery, UserProfileResponse>
{
    public async Task<Result<UserProfileResponse>> Handle(
        GetCurrentUserQuery query,
        CancellationToken cancellationToken)
    {
        UserProfileResponse? user = await dbContext.GetUserProfileAsync(userContext.UserId, cancellationToken);
        return user is null
            ? Result.Failure<UserProfileResponse>(AuthErrors.UserNotFound(userContext.UserId))
            : user;
    }
}
