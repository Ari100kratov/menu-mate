using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;

namespace MenuMate.Modules.Auth.Application.GetCurrentUser;

internal sealed record GetCurrentUserQuery : IQuery<UserProfileResponse>;
