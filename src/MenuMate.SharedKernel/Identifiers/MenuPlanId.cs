namespace MenuMate.SharedKernel.Identifiers;

/// <summary>
/// Идентификатор плана меню, используемый как явная связь с модулем MenuPlanning.
/// </summary>
/// <param name="Value">Значение идентификатора.</param>
public readonly record struct MenuPlanId(Guid Value)
{
    /// <summary>
    /// Создает новый идентификатор плана меню.
    /// </summary>
    public static MenuPlanId Create() => new(Guid.CreateVersion7());

    /// <summary>
    /// Создает идентификатор плана меню из существующего значения.
    /// </summary>
    public static MenuPlanId From(Guid value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
