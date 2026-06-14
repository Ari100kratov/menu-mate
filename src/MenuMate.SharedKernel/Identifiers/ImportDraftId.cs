namespace MenuMate.SharedKernel.Identifiers;

/// <summary>
/// Идентификатор черновика импорта, используемый как явная связь с модулем Imports.
/// </summary>
/// <param name="Value">Значение идентификатора.</param>
public readonly record struct ImportDraftId(Guid Value)
{
    /// <summary>
    /// Создает новый идентификатор черновика импорта.
    /// </summary>
    public static ImportDraftId Create() => new(Guid.CreateVersion7());

    /// <summary>
    /// Создает идентификатор из существующего значения.
    /// </summary>
    public static ImportDraftId From(Guid value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
