using MenuMate.Modules.Recipes.Domain.Models;

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Entities;

internal sealed class RecipeRevisionStepRecord
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid RecipeRevisionId { get; set; }

    public int Number { get; set; }

    public string Text { get; set; } = string.Empty;

    public static RecipeRevisionStepRecord FromDomain(PreparationStep step) =>
        new()
        {
            Number = step.Number,
            Text = step.Text
        };
}
