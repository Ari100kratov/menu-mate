using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.Modules.Recipes.Domain.Models;

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Entities;

internal sealed class RecipeRevisionRecord
{
    public Guid Id { get; set; }

    public Guid RecipeId { get; set; }

    public int RevisionNumber { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int Servings { get; set; }

    public RecipeCategory Category { get; set; }

    public int? TotalTimeMinutes { get; set; }

    public int? ActiveTimeMinutes { get; set; }

    public string? SourceUrl { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public List<RecipeRevisionIngredientRecord> Ingredients { get; set; } = [];

    public List<RecipeRevisionStepRecord> Steps { get; set; } = [];

    public List<RecipeRevisionTagRecord> Tags { get; set; } = [];

    public static RecipeRevisionRecord FromDomain(Recipe recipe)
    {
        var revision = new RecipeRevisionRecord
        {
            Id = recipe.CurrentRevisionId.Value,
            RecipeId = recipe.Id,
            RevisionNumber = recipe.RevisionNumber,
            Title = recipe.Title.Value,
            Description = recipe.Description,
            Servings = recipe.Servings.Value,
            Category = recipe.Category,
            TotalTimeMinutes = recipe.TotalTimeMinutes,
            ActiveTimeMinutes = recipe.ActiveTimeMinutes,
            SourceUrl = recipe.SourceUrl?.ToString(),
            CreatedAt = recipe.UpdatedAt,
            Ingredients =
            [
                .. recipe.Ingredients
                    .Select((ingredient, index) => RecipeRevisionIngredientRecord.FromDomain(ingredient, index))
            ],
            Steps = [.. recipe.Steps.Select(RecipeRevisionStepRecord.FromDomain)],
            Tags = [.. recipe.Tags.Select(RecipeRevisionTagRecord.FromDomain)]
        };

        foreach (RecipeRevisionIngredientRecord ingredient in revision.Ingredients)
        {
            ingredient.RecipeRevisionId = revision.Id;
        }

        foreach (RecipeRevisionStepRecord step in revision.Steps)
        {
            step.RecipeRevisionId = revision.Id;
        }

        foreach (RecipeRevisionTagRecord tag in revision.Tags)
        {
            tag.RecipeRevisionId = revision.Id;
        }

        return revision;
    }
}
