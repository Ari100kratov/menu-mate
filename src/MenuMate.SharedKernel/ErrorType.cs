namespace MenuMate.SharedKernel;

/// <summary>
/// Определяет категории ошибок для доменного, прикладного и API-слоев.
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// Общая ошибка операции.
    /// </summary>
    Failure = 0,

    /// <summary>
    /// Ошибка проверки входных данных или доменного состояния.
    /// </summary>
    Validation = 1,

    /// <summary>
    /// Запрошенная сущность не найдена.
    /// </summary>
    NotFound = 2,

    /// <summary>
    /// Операция конфликтует с текущим состоянием системы.
    /// </summary>
    Conflict = 3,

    /// <summary>
    /// Пользователь не аутентифицирован.
    /// </summary>
    Unauthorized = 4,

    /// <summary>
    /// Аутентифицированный пользователь не может получить доступ к ресурсу.
    /// </summary>
    Forbidden = 5,

    /// <summary>
    /// Ошибка инфраструктуры или внешней интеграции.
    /// </summary>
    Problem = 6
}
