using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.ShoppingLists.Infrastructure.Database.Source;

internal sealed class MenuPlanItemSourceRecord
{
    public Guid Id { get; set; }

    public MenuPlanId MenuPlanId { get; set; }

    public int Servings { get; set; }

    public RecipeId? RecipeId { get; set; }
}
