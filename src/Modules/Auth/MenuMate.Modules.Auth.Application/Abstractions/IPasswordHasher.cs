namespace MenuMate.Modules.Auth.Application.Abstractions;

/// <summary>
/// Хэширует и проверяет пароли.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Хэширует пароль.
    /// </summary>
    string Hash(string password);

    /// <summary>
    /// Проверяет пароль по сохраненному хэшу.
    /// </summary>
    bool Verify(string password, string passwordHash);
}
