using MenuMate.Modules.Recipes.Domain.ValueObjects;

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Entities;

internal sealed class RecipeRevisionTagRecord
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid RecipeRevisionId { get; set; }

    public string Value { get; set; } = string.Empty;

    public string NormalizedValue { get; set; } = string.Empty;

    public static RecipeRevisionTagRecord FromDomain(RecipeTag tag) =>
        new()
        {
            Value = tag.Value,
            NormalizedValue = tag.NormalizedValue
        };
}
