namespace MenuMate.Modules.Recipes.Infrastructure.Database.Source;

internal sealed class RecipeMenuItemSourceRecord
{
    public Guid Id { get; set; }

    public Guid OwnerUserId { get; set; }

    public Guid? RecipeId { get; set; }

    public Guid? RecipeRevisionId { get; set; }
}
