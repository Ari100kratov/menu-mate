namespace MenuMate.SharedKernel;

/// <summary>
/// Исключение для нарушений доменных инвариантов, которые не должны доходить до сохранения.
/// </summary>
public sealed class DomainException : Exception
{
    /// <summary>
    /// Создает экземпляр <see cref="DomainException" />.
    /// </summary>
    public DomainException()
        : this(AppError.Failure("Domain.Exception", "A domain invariant was violated."))
    {
    }

    /// <summary>
    /// Создает экземпляр <see cref="DomainException" />.
    /// </summary>
    public DomainException(string message)
        : this(AppError.Failure("Domain.Exception", message))
    {
    }

    /// <summary>
    /// Создает экземпляр <see cref="DomainException" />.
    /// </summary>
    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
        Error = AppError.Failure("Domain.Exception", message);
    }

    /// <summary>
    /// Создает экземпляр <see cref="DomainException" />.
    /// </summary>
    public DomainException(AppError error)
        : base((error ?? throw new ArgumentNullException(nameof(error))).Description)
    {
        Error = error;
    }

    /// <summary>
    /// Доменная ошибка.
    /// </summary>
    public AppError Error { get; }
}
