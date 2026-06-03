namespace MenuMate.SharedKernel.Identifiers;

/// <summary>
/// Идентификатор пользователя, используемый как явная связь с модулем Auth.
/// </summary>
/// <param name="Value">Значение идентификатора.</param>
public readonly record struct UserId(Guid Value)
{
    /// <summary>
    /// Создает новый идентификатор пользователя.
    /// </summary>
    public static UserId Create() => new(Guid.CreateVersion7());

    /// <summary>
    /// Создает идентификатор пользователя из существующего значения.
    /// </summary>
    public static UserId From(Guid value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
