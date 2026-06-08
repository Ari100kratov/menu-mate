namespace MenuMate.SharedKernel.Identifiers;

/// <summary>
/// Identifies an immutable recipe revision shared with menu planning and shopping lists.
/// </summary>
public readonly record struct RecipeRevisionId(Guid Value)
{
    /// <summary>Creates a new revision identifier.</summary>
    public static RecipeRevisionId Create() => new(Guid.CreateVersion7());

    /// <summary>Creates a revision identifier from an existing value.</summary>
    public static RecipeRevisionId From(Guid value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
