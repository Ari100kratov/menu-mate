using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Common.Application;

/// <summary>
/// Предоставляет текущего аутентифицированного пользователя для обработчиков приложения.
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// Идентификатор текущего пользователя.
    /// </summary>
    UserId UserId { get; }
}
