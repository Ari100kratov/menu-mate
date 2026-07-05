using System.Text.RegularExpressions;
using MenuMate.Contracts.Recipes;

namespace MenuMate.DataImporter.Recipes;

internal static partial class ImportedRecipeValidator
{
    public static string? Validate(CreateRecipeRequest? recipe)
    {
        if (recipe is null)
        {
            return "Страница не распознана как рецепт.";
        }

        if (!ContainsCyrillicRegex().IsMatch(recipe.Title))
        {
            return "Название рецепта не является русскоязычным.";
        }

        if (recipe.Ingredients.Count == 0)
        {
            return "В рецепте не найдены ингредиенты.";
        }

        if (recipe.Steps.Count == 0)
        {
            return "В рецепте не найдены шаги приготовления.";
        }

        if (recipe.Ingredients.Any(ingredient =>
                string.IsNullOrWhiteSpace(ingredient.ProductName) ||
                ingredient.Unit == "Unknown" ||
                ingredient.Category == "Other"))
        {
            return "Не удалось однозначно определить продукты, единицы или категории.";
        }

        if (recipe.Steps.Any(step => string.IsNullOrWhiteSpace(step.Text)))
        {
            return "В рецепте найден пустой шаг приготовления.";
        }

        return null;
    }

    [GeneratedRegex("[А-Яа-яЁё]", RegexOptions.CultureInvariant)]
    private static partial Regex ContainsCyrillicRegex();
}
