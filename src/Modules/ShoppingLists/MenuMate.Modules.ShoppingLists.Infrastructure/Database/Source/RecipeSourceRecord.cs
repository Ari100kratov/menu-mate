using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.ShoppingLists.Infrastructure.Database.Source;

internal sealed class RecipeSourceRecord
{
    public RecipeId Id { get; set; }

    public UserId OwnerUserId { get; set; }

    public int Servings { get; set; }

    public bool IsDeleted { get; set; }

    public List<RecipeIngredientSourceRecord> Ingredients { get; set; } = [];
}
