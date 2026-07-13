using System.Text.RegularExpressions;
using MenuMate.Contracts.Recipes;
using MenuMate.SharedKernel;

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
                .ToArray(),
            Tags = NormalizeTags(recipe.Tags, recipe.Title)
        };
    }

    private static RecipeIngredientRequest NormalizeIngredientQuantity(RecipeIngredientRequest ingredient)
    {
        RecipeIngredientRequest normalizedIngredient = ingredient with
        {
            ProductName = ProductNameNormalizer.Normalize(ingredient.ProductName)
        };

        if (normalizedIngredient.Amount is null
            && normalizedIngredient.Comment is not null
            && PinchCommentRegex().IsMatch(normalizedIngredient.Comment))
        {
            return normalizedIngredient with
            {
                Amount = 1m,
                Unit = "Pinch",
                Comment = null,
            };
        }

        return normalizedIngredient.Amount is null
            ? normalizedIngredient with { Unit = "ToTaste" }
            : normalizedIngredient;
    }

    private static string[] NormalizeTags(
        IReadOnlyCollection<string> tags,
        string title)
    {
        string normalizedTitle = ProductNameNormalizer.Normalize(title);

        return
        [.. tags
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(ProductNameNormalizer.Normalize)
            .Where(tag => tag != normalizedTitle && !RecipeTypeTags.Contains(tag))
            .Distinct(StringComparer.Ordinal)
            .Take(6)];
    }

    private static readonly HashSet<string> RecipeTypeTags =
    [
        "завтрак", "обед", "ужин", "перекус", "суп", "основное блюдо", "гарнир",
        "салат", "закуска", "десерт", "выпечка", "напиток", "соус", "другое",
        "breakfast", "lunch", "dinner", "snack", "soup", "main course", "main dish",
        "side dish", "salad", "appetizer", "dessert", "baking", "drink", "sauce", "other",
        "еда", "рецепт", "food", "recipe"
    ];

    [GeneratedRegex(
        @"^\s*(?:(?:(?:шаг|step)\s*)?\d+\s*[.)\]:\-–—]\s*|(?:шаг|step)\s*\d+\s+)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex StepPrefixRegex();

    [GeneratedRegex(@"^\s*(?:одна\s+)?щепотка\s*[.!]?\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PinchCommentRegex();
}
