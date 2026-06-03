using MenuMate.Modules.Recipes.Domain.ValueObjects;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Entities;

internal sealed class RecipeTagRecord
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid RecipeId { get; set; }

    public string Value { get; set; } = string.Empty;

    public string NormalizedValue { get; set; } = string.Empty;

    public static RecipeTagRecord FromDomain(RecipeTag tag) =>
        new()
        {
            Value = tag.Value,
            NormalizedValue = tag.NormalizedValue
        };

    public RecipeTag ToDomain()
    {
        Result<RecipeTag> tag = RecipeTag.Create(Value);
        return tag.IsSuccess ? tag.Value : throw new DomainException(tag.Error);
    }
}
