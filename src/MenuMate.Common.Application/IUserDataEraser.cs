using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Common.Application;

/// <summary>
/// Идемпотентно удаляет данные пользователя, принадлежащие отдельному модулю.
/// </summary>
public interface IUserDataEraser
{
    /// <summary>Порядок удаления с учетом межмодульных ссылок.</summary>
    int Order { get; }

    /// <summary>Удаляет данные указанного пользователя.</summary>
    Task EraseAsync(UserId userId, CancellationToken cancellationToken);
}
