namespace MenuMate.SharedKernel.Identifiers;

/// <summary>
/// Идентификатор рецепта, используемый как явная связь с модулем Recipes.
/// </summary>
/// <param name="Value">Значение идентификатора.</param>
public readonly record struct RecipeId(Guid Value)
{
    /// <summary>
    /// Создает новый идентификатор рецепта.
    /// </summary>
    public static RecipeId Create() => new(Guid.CreateVersion7());

    /// <summary>
    /// Создает идентификатор рецепта из существующего значения.
    /// </summary>
    public static RecipeId From(Guid value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
