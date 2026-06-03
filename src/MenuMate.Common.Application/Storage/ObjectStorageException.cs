namespace MenuMate.Common.Application.Storage;

/// <summary>
/// Базовое исключение объектного хранилища.
/// </summary>
public abstract class ObjectStorageException : Exception
{
    /// <summary>
    /// Создает исключение объектного хранилища.
    /// </summary>
    protected ObjectStorageException()
    {
    }

    /// <summary>
    /// Создает исключение объектного хранилища с сообщением.
    /// </summary>
    protected ObjectStorageException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Создает исключение объектного хранилища с сообщением и внутренним исключением.
    /// </summary>
    protected ObjectStorageException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Бакет не найден.
/// </summary>
public sealed class ObjectStorageBucketNotFoundException : ObjectStorageException
{
    /// <summary>
    /// Создает исключение отсутствующего бакета.
    /// </summary>
    public ObjectStorageBucketNotFoundException()
    {
    }

    /// <summary>
    /// Создает исключение отсутствующего бакета по имени бакета.
    /// </summary>
    public ObjectStorageBucketNotFoundException(string bucket)
        : base($"Бакет '{bucket}' не найден.")
    {
    }

    /// <summary>
    /// Создает исключение отсутствующего бакета с сообщением и внутренним исключением.
    /// </summary>
    public ObjectStorageBucketNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Объект не найден.
/// </summary>
public sealed class ObjectStorageObjectNotFoundException : ObjectStorageException
{
    /// <summary>
    /// Создает исключение отсутствующего объекта.
    /// </summary>
    public ObjectStorageObjectNotFoundException()
    {
    }

    /// <summary>
    /// Создает исключение отсутствующего объекта с сообщением.
    /// </summary>
    public ObjectStorageObjectNotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Создает исключение отсутствующего объекта по бакету и ключу.
    /// </summary>
    public ObjectStorageObjectNotFoundException(string bucket, string key)
        : base($"Объект '{key}' не найден в бакете '{bucket}'.")
    {
    }

    /// <summary>
    /// Создает исключение отсутствующего объекта с сообщением и внутренним исключением.
    /// </summary>
    public ObjectStorageObjectNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Недостаточно прав для доступа к хранилищу.
/// </summary>
public sealed class ObjectStorageAuthorizationException : ObjectStorageException
{
    /// <summary>
    /// Создает исключение авторизации.
    /// </summary>
    public ObjectStorageAuthorizationException()
        : base("Нет прав для доступа к объектному хранилищу.")
    {
    }

    /// <summary>
    /// Создает исключение авторизации с сообщением.
    /// </summary>
    public ObjectStorageAuthorizationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Создает исключение авторизации с сообщением и внутренним исключением.
    /// </summary>
    public ObjectStorageAuthorizationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Некорректное имя бакета или объекта.
/// </summary>
public sealed class ObjectStorageInvalidNameException : ObjectStorageException
{
    /// <summary>
    /// Создает исключение некорректного имени.
    /// </summary>
    public ObjectStorageInvalidNameException()
    {
    }

    /// <summary>
    /// Создает исключение некорректного имени.
    /// </summary>
    public ObjectStorageInvalidNameException(string name)
        : base($"Некорректное имя объекта или бакета: {name}.")
    {
    }

    /// <summary>
    /// Создает исключение некорректного имени с сообщением и внутренним исключением.
    /// </summary>
    public ObjectStorageInvalidNameException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Ошибка чтения из объектного хранилища.
/// </summary>
public sealed class ObjectStorageReadException : ObjectStorageException
{
    /// <summary>
    /// Создает исключение чтения из хранилища.
    /// </summary>
    public ObjectStorageReadException()
    {
    }

    /// <summary>
    /// Создает исключение чтения из хранилища.
    /// </summary>
    public ObjectStorageReadException(string reason)
        : base($"Ошибка чтения из объектного хранилища: {reason}")
    {
    }

    /// <summary>
    /// Создает исключение чтения из хранилища с сообщением и внутренним исключением.
    /// </summary>
    public ObjectStorageReadException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Ошибка записи в объектное хранилище.
/// </summary>
public sealed class ObjectStorageWriteException : ObjectStorageException
{
    /// <summary>
    /// Создает исключение записи в хранилище.
    /// </summary>
    public ObjectStorageWriteException()
    {
    }

    /// <summary>
    /// Создает исключение записи в хранилище.
    /// </summary>
    public ObjectStorageWriteException(string reason)
        : base($"Ошибка записи в объектное хранилище: {reason}")
    {
    }

    /// <summary>
    /// Создает исключение записи в хранилище с сообщением и внутренним исключением.
    /// </summary>
    public ObjectStorageWriteException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
