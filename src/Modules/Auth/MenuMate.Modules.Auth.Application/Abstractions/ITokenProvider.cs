using MenuMate.Modules.Auth.Domain.Models;

namespace MenuMate.Modules.Auth.Application.Abstractions;

/// <summary>
/// Issues access tokens.
/// </summary>
public interface ITokenProvider
{
    /// <summary>
    /// Создает JWT-токен доступа.
    /// </summary>
    AccessToken Create(User user);
}
