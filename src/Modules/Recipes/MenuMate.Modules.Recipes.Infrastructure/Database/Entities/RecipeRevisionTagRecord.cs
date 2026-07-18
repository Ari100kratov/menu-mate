using MenuMate.Modules.Recipes.Domain.ValueObjects;

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Entities;

internal sealed class RecipeRevisionTagRecord
{
    public Guid RecipeRevisionId { get; set; }

    public Guid TagId { get; set; }

    public static RecipeRevisionTagRecord FromDomain(RecipeTag tag) =>
        new()
        {
            TagId = tag.Id
        };
}
