using MenuMate.Modules.Recipes.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Entities;

internal sealed class PreparationStepRecord
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid RecipeId { get; set; }

    public int Number { get; set; }

    public string Text { get; set; } = string.Empty;

    public static PreparationStepRecord FromDomain(PreparationStep step) =>
        new()
        {
            Number = step.Number,
            Text = step.Text
        };

    public PreparationStep ToDomain()
    {
        Result<PreparationStep> step = PreparationStep.Create(Number, Text);
        return step.IsSuccess ? step.Value : throw new DomainException(step.Error);
    }
}
