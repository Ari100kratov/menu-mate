using MenuMate.Contracts.Auth;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.Auth.Application.Abstractions;

/// <summary>
/// Контракт чтения профиля пользователя через EF-проекцию без гидрации доменного агрегата.
/// </summary>
internal interface IAuthReadDbContext
{
    /// <summary>
    /// Возвращает публичный профиль пользователя.
    /// </summary>
    Task<UserProfileResponse?> GetUserProfileAsync(UserId userId, CancellationToken cancellationToken);
}
