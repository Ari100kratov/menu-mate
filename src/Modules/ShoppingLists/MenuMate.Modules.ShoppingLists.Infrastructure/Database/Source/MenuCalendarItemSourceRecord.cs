using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.ShoppingLists.Infrastructure.Database.Source;

internal sealed class MenuCalendarItemSourceRecord
{
    public Guid Id { get; set; }

    public UserId OwnerUserId { get; set; }

    public DateOnly Date { get; set; }

    public int Servings { get; set; }

    public RecipeId? RecipeId { get; set; }

    public RecipeRevisionId? RecipeRevisionId { get; set; }

    public string? RecipeTitle { get; set; }
}
