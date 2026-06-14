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
            Steps = recipe.Steps
                .Select(step => new PreparationStepRequest(StepPrefixRegex().Replace(step.Text, string.Empty).Trim()))
                .ToArray()
        };
    }

    [GeneratedRegex(
        @"^\s*(?:(?:(?:шаг|step)\s*)?\d+\s*[.)\]:\-–—]\s*|(?:шаг|step)\s*\d+\s+)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex StepPrefixRegex();
}
