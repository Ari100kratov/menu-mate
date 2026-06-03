using System.Diagnostics.CodeAnalysis;

namespace MenuMate.SharedKernel;

/// <summary>
/// Представляет результат операции без возвращаемого значения.
/// </summary>
public class Result
{
    /// <summary>
    /// Инициализирует новый экземпляр <see cref="Result" />.
    /// </summary>
    protected Result(bool isSuccess, AppError error)
    {
        if (isSuccess && error != AppError.None || !isSuccess && error == AppError.None)
        {
            throw new ArgumentException("Некорректная комбинация успеха и ошибки.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Возвращает true, если операция завершилась успешно.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Возвращает true, если операция завершилась ошибкой.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Ошибка операции или <see cref="AppError.None" /> для успешного результата.
    /// </summary>
    public AppError Error { get; }

    /// <summary>
    /// Создает успешный результат.
    /// </summary>
    public static Result Success() => new(true, AppError.None);

    /// <summary>
    /// Создает успешный результат со значением.
    /// </summary>
    public static Result<TValue> Success<TValue>(TValue value) =>
        new(value, true, AppError.None);

    /// <summary>
    /// Создает неуспешный результат.
    /// </summary>
    public static Result Failure(AppError error) => new(false, error);

    /// <summary>
    /// Создает неуспешный результат с ожидаемым типом значения.
    /// </summary>
    public static Result<TValue> Failure<TValue>(AppError error) =>
        new(default, false, error);

}

/// <summary>
/// Представляет результат операции с возвращаемым значением.
/// </summary>
/// <typeparam name="TValue">Тип значения успешного результата.</typeparam>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, AppError error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Значение успешного результата.
    /// </summary>
    [NotNull]
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Нельзя получить значение неуспешного результата.");

    /// <summary>
    /// Создает результат из nullable-значения.
    /// </summary>
    [SuppressMessage(
        "Design",
        "CA1000:Do not declare static members on generic types",
        Justification = "Named alternative is required for the implicit conversion operator by CA2225.")]
    public static Result<TValue> FromTValue(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(AppError.NullValue);

    /// <summary>
    /// Преобразует значение в успешный результат, если оно не null.
    /// </summary>
    public static implicit operator Result<TValue>(TValue? value) =>
        FromTValue(value);
}


