using MenuMate.Contracts.Recipes;
using MenuMate.SharedKernel;

namespace MenuMate.DataImporter.Recipes;

internal static class IngredientAliasNormalizer
{
    private static readonly Dictionary<string, string> Aliases =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [TextNormalizer.NormalizeSearchText("лук репчатый")] = "репчатый лук",
            [TextNormalizer.NormalizeSearchText("луковица")] = "репчатый лук",
            [TextNormalizer.NormalizeSearchText("картошка")] = "картофель",
            [TextNormalizer.NormalizeSearchText("помидор")] = "томат",
            [TextNormalizer.NormalizeSearchText("помидоры")] = "томаты",
            [TextNormalizer.NormalizeSearchText("сахарный песок")] = "сахар",
            [TextNormalizer.NormalizeSearchText("масло подсолнечное")] = "подсолнечное масло",
            [TextNormalizer.NormalizeSearchText("масло оливковое")] = "оливковое масло",
            [TextNormalizer.NormalizeSearchText("перец черный")] = "черный перец",
            [TextNormalizer.NormalizeSearchText("перец чёрный")] = "черный перец",
            [TextNormalizer.NormalizeSearchText("мука пшеничная")] = "пшеничная мука",
            [TextNormalizer.NormalizeSearchText("яйцо куриное")] = "куриное яйцо",
            [TextNormalizer.NormalizeSearchText("чесночный зубчик")] = "чеснок",
            [TextNormalizer.NormalizeSearchText("кинза")] = "кориандр свежий",
            [TextNormalizer.NormalizeSearchText("нут турецкий")] = "нут",
            [TextNormalizer.NormalizeSearchText("баклажан")] = "баклажан",
            [TextNormalizer.NormalizeSearchText("свёкла")] = "свекла"
        };

    public static CreateRecipeRequest Normalize(CreateRecipeRequest recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        return recipe with
        {
            Ingredients = recipe.Ingredients
                .Select(ingredient =>
                {
                    string normalized = TextNormalizer.NormalizeSearchText(ingredient.ProductName);
                    return Aliases.TryGetValue(normalized, out string? canonical)
                        ? ingredient with { ProductName = canonical }
                        : ingredient;
                })
                .ToArray()
        };
    }
}
