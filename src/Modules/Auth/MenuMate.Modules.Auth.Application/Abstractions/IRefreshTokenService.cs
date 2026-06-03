using MenuMate.Modules.Auth.Domain.Models;

namespace MenuMate.Modules.Auth.Application.Abstractions;

/// <summary>
/// Создает и проверяет refresh tokens.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Создает refresh-токен для пользователя.
    /// </summary>
    RefreshToken Generate(User user, int daysValid = 7);

    /// <summary>
    /// Возвращает true, если refresh token можно использовать.
    /// </summary>
    bool IsValid(RefreshToken token);
}
