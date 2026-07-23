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

    /// <summary>
    /// Возвращает страницу зарегистрированных пользователей для административного просмотра.
    /// </summary>
    Task<AdminUsersPageReadModel> GetAdminUsersAsync(
        string? search,
        int skip,
        int take,
        CancellationToken cancellationToken);
}

/// <summary>
/// Страница пользователей, полученная из хранилища Auth.
/// </summary>
internal sealed record AdminUsersPageReadModel(
    int TotalCount,
    IReadOnlyCollection<AdminUserReadModel> Users);

/// <summary>
/// Данные пользователя без статистики из смежных модулей.
/// </summary>
internal sealed record AdminUserReadModel(
    Guid Id,
    string Email,
    string DisplayName,
    DateTimeOffset RegisteredAt,
    IReadOnlyCollection<string> Roles);
