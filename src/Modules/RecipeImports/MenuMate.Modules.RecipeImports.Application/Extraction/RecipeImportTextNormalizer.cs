using System.Text.RegularExpressions;
using MenuMate.Contracts.Recipes;

namespace MenuMate.Modules.RecipeImports.Application.Extraction;

/// <summary>
/// Нормализует текст, полученный при распознавании рецепта.
/// </summary>
public static partial class RecipeImportTextNormalizer
{
    /// <summary>Удаляет дублирующую порядковую нумерацию из текста шагов.</summary>
    public static CreateRecipeRequest Normalize(CreateRecipeRequest recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        return recipe with
        {
            Ingredients = recipe.Ingredients
                .Select(NormalizeIngredientQuantity)
                .ToArray(),
            Steps = recipe.Steps
                .Select(step => new PreparationStepRequest(StepPrefixRegex().Replace(step.Text, string.Empty).Trim()))
                .ToArray()
        };
    }

    private static RecipeIngredientRequest NormalizeIngredientQuantity(RecipeIngredientRequest ingredient)
    {
        if (ingredient.Amount is null
            && ingredient.Comment is not null
            && PinchCommentRegex().IsMatch(ingredient.Comment))
        {
            return ingredient with
            {
                Amount = 1m,
                Unit = "Pinch",
                Comment = null,
            };
        }

        return ingredient.Amount is null
            ? ingredient with { Unit = "ToTaste" }
            : ingredient;
    }

    [GeneratedRegex(
        @"^\s*(?:(?:(?:шаг|step)\s*)?\d+\s*[.)\]:\-–—]\s*|(?:шаг|step)\s*\d+\s+)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex StepPrefixRegex();

    [GeneratedRegex(@"^\s*(?:одна\s+)?щепотка\s*[.!]?\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PinchCommentRegex();
}
