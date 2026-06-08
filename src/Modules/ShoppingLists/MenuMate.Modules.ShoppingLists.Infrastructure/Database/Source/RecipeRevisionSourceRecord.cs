using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.ShoppingLists.Infrastructure.Database.Source;

internal sealed class RecipeRevisionSourceRecord
{
    public RecipeRevisionId Id { get; set; }

    public RecipeId RecipeId { get; set; }

    public int Servings { get; set; }

    public List<RecipeRevisionIngredientSourceRecord> Ingredients { get; set; } = [];
}
