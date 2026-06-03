using MenuMate.Common.Application;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.AspNetCore.Http;

namespace MenuMate.Modules.Auth.Infrastructure.Authentication;

internal sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public UserId UserId => new(
        httpContextAccessor.HttpContext?.User.GetUserId()
        ?? throw new InvalidOperationException("Контекст пользователя недоступен."));
}
