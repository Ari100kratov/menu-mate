namespace MenuMate.SharedKernel;

/// <summary>
/// Машиночитаемое описание ошибки, возвращаемой доменом и use case-ами.
/// </summary>
public sealed record AppError(string Code, string Description, ErrorType Type)
{
    /// <summary>
    /// Отсутствие ошибки для успешного результата.
    /// </summary>
    public static readonly AppError None = new(string.Empty, string.Empty, ErrorType.Failure);

    /// <summary>
    /// Ошибка, используемая при попытке создать успешный результат из null.
    /// </summary>
    public static readonly AppError NullValue = new(
        "General.NullValue",
        "Значение не может быть null.",
        ErrorType.Failure);

    /// <summary>
    /// Создает ошибку общего сбоя.
    /// </summary>
    public static AppError Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);

    /// <summary>
    /// Создает ошибку валидации.
    /// </summary>
    public static AppError Validation(string code, string description) =>
        new(code, description, ErrorType.Validation);

    /// <summary>
    /// Создает ошибку отсутствующей сущности.
    /// </summary>
    public static AppError NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    /// <summary>
    /// Создает ошибку конфликта состояния.
    /// </summary>
    public static AppError Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    /// <summary>
    /// Создает ошибку аутентификации.
    /// </summary>
    public static AppError Unauthorized(string code, string description) =>
        new(code, description, ErrorType.Unauthorized);

    /// <summary>
    /// Создает ошибку авторизации.
    /// </summary>
    public static AppError Forbidden(string code, string description) =>
        new(code, description, ErrorType.Forbidden);

    /// <summary>
    /// Создает ошибку инфраструктурной проблемы.
    /// </summary>
    public static AppError Problem(string code, string description) =>
        new(code, description, ErrorType.Problem);
}


