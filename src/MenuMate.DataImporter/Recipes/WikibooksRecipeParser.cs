using System.Globalization;
using System.Text.RegularExpressions;
using MenuMate.Common.Application.Products;
using MenuMate.Contracts.Recipes;
using MenuMate.DataImporter.Wikibooks;

namespace MenuMate.DataImporter.Recipes;

internal static partial class WikibooksRecipeParser
{
    public static CreateRecipeRequest? Parse(WikibooksPage page)
    {
        ArgumentNullException.ThrowIfNull(page);

        var ingredientLines = new List<string>();
        var stepLines = new List<string>();
        Section section = Section.None;

        foreach (string rawLine in page.Wikitext.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            string line = rawLine.Trim();
            Match heading = HeadingRegex().Match(line);
            if (heading.Success)
            {
                Section nextSection = ClassifySection(heading.Groups[1].Value);
                if (section == Section.Steps &&
                    nextSection != Section.Steps &&
                    ingredientLines.Count > 0 &&
                    stepLines.Count > 0)
                {
                    break;
                }

                section = nextSection;
                continue;
            }

            if (section == Section.Ingredients && BulletRegex().IsMatch(line))
            {
                ingredientLines.Add(BulletRegex().Replace(line, string.Empty));
            }
            else if (section == Section.Steps && BulletOrNumberRegex().IsMatch(line))
            {
                stepLines.Add(BulletOrNumberRegex().Replace(line, string.Empty));
            }
        }

        RecipeIngredientRequest[] ingredients =
        [
            .. ingredientLines
            .Select(ParseStructuredIngredient)
            .Where(ingredient => ingredient is not null)
            .Cast<RecipeIngredientRequest>()
        ];
        PreparationStepRequest[] steps =
        [
            .. stepLines
            .Select(RecipeTextNormalizer.NormalizeStep)
            .Where(step => !string.IsNullOrWhiteSpace(step))
            .Select(step => new PreparationStepRequest(step))
        ];

        if (ingredients.Length == 0 || steps.Length == 0)
        {
            return ParseFirstFreeFormVariant(page);
        }

        return CreateRecipe(page, NormalizeTitle(page.Title), ingredients, steps);
    }

    private static CreateRecipeRequest? ParseFirstFreeFormVariant(WikibooksPage page)
    {
        string[] lines = page.Wikitext.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        int index = 0;
        while (index < lines.Length)
        {
            Match heading = HeadingRegex().Match(lines[index].Trim());
            if (!heading.Success)
            {
                index++;
                continue;
            }

            string title = RecipeTextNormalizer.CleanWikiMarkup(heading.Groups[1].Value);
            var paragraphs = new List<string>();
            var paragraphLines = new List<string>();
            index++;
            while (index < lines.Length)
            {
                string line = lines[index].Trim();
                if (HeadingRegex().IsMatch(line))
                {
                    AddParagraph(paragraphs, paragraphLines);
                    break;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    AddParagraph(paragraphs, paragraphLines);
                }
                else if (!IsTechnicalLine(line))
                {
                    paragraphLines.Add(line);
                }

                index++;
            }

            AddParagraph(paragraphs, paragraphLines);
            CreateRecipeRequest? recipe = ParseFreeFormVariant(page, title, paragraphs);
            if (recipe is not null)
            {
                return recipe;
            }
        }

        return null;
    }

    private static CreateRecipeRequest? ParseFreeFormVariant(
        WikibooksPage page,
        string title,
        List<string> paragraphs)
    {
        for (int index = 0; index < paragraphs.Count - 1; index++)
        {
            RecipeIngredientRequest[] ingredients =
            [
                .. SplitIngredientList(paragraphs[index])
                    .Select(ParseIngredient)
                    .Where(ingredient => ingredient is not null && ingredient.Unit != "Unknown")
                    .Cast<RecipeIngredientRequest>()
            ];
            if (ingredients.Length < 3)
            {
                continue;
            }

            PreparationStepRequest[] steps =
            [
                .. paragraphs
                    .Skip(index + 1)
                    .Select(RecipeTextNormalizer.NormalizeStep)
                    .Where(step => !string.IsNullOrWhiteSpace(step))
                    .Select(step => new PreparationStepRequest(step))
            ];
            if (steps.Length > 0)
            {
                return CreateRecipe(page, title, ingredients, steps);
            }
        }

        return null;
    }

    private static CreateRecipeRequest CreateRecipe(
        WikibooksPage page,
        string title,
        IReadOnlyCollection<RecipeIngredientRequest> ingredients,
        IReadOnlyCollection<PreparationStepRequest> steps) =>
        new(
            title,
            null,
            1,
            ClassifyRecipeCategory($"{page.Title}\n{page.Wikitext}"),
            "Public",
            null,
            null,
            page.SourceUrl,
            ingredients,
            steps,
            []);

    private static IEnumerable<string> SplitIngredientList(string paragraph) =>
        InlineIngredientSeparatorRegex()
            .Split(paragraph)
            .Select(value => value.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value));

    private static void AddParagraph(List<string> paragraphs, List<string> paragraphLines)
    {
        if (paragraphLines.Count == 0)
        {
            return;
        }

        paragraphs.Add(string.Join(' ', paragraphLines));
        paragraphLines.Clear();
    }

    private static bool IsTechnicalLine(string line) =>
        line.StartsWith("{{", StringComparison.Ordinal) ||
        line.StartsWith("[[Категория:", StringComparison.OrdinalIgnoreCase) ||
        line.StartsWith("__", StringComparison.Ordinal);

    private static string NormalizeTitle(string title)
    {
        string cleaned = RecipeTextNormalizer.CleanWikiMarkup(title);
        return RecipeNamespacePrefixRegex().Replace(cleaned, string.Empty).Trim();
    }

    private static RecipeIngredientRequest? ParseIngredient(string line)
    {
        string cleaned = RecipeTextNormalizer.CleanWikiMarkup(line);
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            return null;
        }

        Match match = NameFirstIngredientRegex().Match(cleaned);
        bool nameFirst = match.Success;
        if (!match.Success)
        {
            match = IngredientRegex().Match(cleaned);
        }

        if (!match.Success)
        {
            return new RecipeIngredientRequest(null, cleaned, 1, "Unknown", ProductCategoryClassifier.GetCategory(cleaned), null, false);
        }

        string rawAmount = match.Groups["amount"].Value;
        decimal? amount = ParseAmount(rawAmount);
        string rawUnit = match.Groups["unit"].Value;
        string name = match.Groups["name"].Value.Trim(' ', '-', '—', ',');
        string unit = ParseUnit(rawUnit);
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        if (unit == "ToTaste")
        {
            amount = null;
        }
        else
        {
            amount ??= 1;
        }

        return new RecipeIngredientRequest(
            null,
            name,
            amount,
            unit,
            ProductCategoryClassifier.GetCategory(name),
            CreateIngredientComment(match, rawAmount, rawUnit, nameFirst),
            cleaned.Contains("по желанию", StringComparison.OrdinalIgnoreCase));
    }

    private static RecipeIngredientRequest? ParseStructuredIngredient(string line)
    {
        RecipeIngredientRequest? ingredient = ParseIngredient(line);
        return ingredient?.Unit == "Unknown"
            ? ingredient with { Amount = null, Unit = "ToTaste" }
            : ingredient;
    }

    private static string? CreateIngredientComment(Match match, string amount, string unit, bool nameFirst)
    {
        var comments = new List<string>();
        if (RangeAmountRegex().IsMatch(amount))
        {
            comments.Add($"Количество в источнике: {amount} {unit}".Trim());
        }

        if (nameFirst)
        {
            string suffix = match.Groups["suffix"].Value.Trim();
            if (!string.IsNullOrWhiteSpace(suffix))
            {
                comments.Add(suffix);
            }
        }

        return comments.Count == 0 ? null : string.Join(". ", comments);
    }

    private static decimal? ParseAmount(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string normalized = RangeSeparatorRegex().Split(value.Replace(',', '.'))[0];
        if (normalized.Contains('/', StringComparison.Ordinal))
        {
            string[] parts = normalized.Split('/');
            return parts.Length == 2 &&
                decimal.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal numerator) &&
                decimal.TryParse(parts[1], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal denominator) &&
                denominator != 0
                    ? numerator / denominator
                    : null;
        }

        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount)
            ? amount
            : null;
    }

    private static string ParseUnit(string value)
    {
        string normalized = value.Trim().ToUpperInvariant().TrimEnd('.');
        return normalized switch
        {
            "Г" or "ГР" or "ГРАММ" or "ГРАММА" or "ГРАММОВ" => "Gram",
            "КГ" or "КИЛОГРАММ" or "КИЛОГРАММА" or "КИЛОГРАММОВ" => "Kilogram",
            "МЛ" or "МИЛЛИЛИТР" or "МИЛЛИЛИТРА" or "МИЛЛИЛИТРОВ" => "Milliliter",
            "Л" or "ЛИТР" or "ЛИТРА" or "ЛИТРОВ" => "Liter",
            "ШТ" or "ШТУКА" or "ШТУКИ" or "ШТУК" => "Piece",
            "Ч. Л" or "Ч Л" or "ЧАЙНАЯ ЛОЖКА" or "ЧАЙНЫЕ ЛОЖКИ" => "Teaspoon",
            "СТ. Л" or "СТ Л" or "СТОЛОВАЯ ЛОЖКА" or "СТОЛОВЫЕ ЛОЖКИ" => "Tablespoon",
            "ДЕС. Л" or "ДЕС Л" or "ДЕСЕРТНАЯ ЛОЖКА" => "Dessertspoon",
            "СТАКАН" or "СТАКАНА" or "СТАКАНОВ" => "Glass",
            "ЧАШКА" or "ЧАШКИ" or "ЧАШЕК" => "Cup",
            "ЗУБЧИК" or "ЗУБЧИКА" or "ЗУБЧИКОВ" => "Clove",
            "ПУЧОК" or "ПУЧКА" or "ПУЧКОВ" => "Bunch",
            "ВЕТОЧКА" or "ВЕТОЧКИ" or "ВЕТОЧЕК" => "Sprig",
            "ГОЛОВКА" or "ГОЛОВКИ" or "ГОЛОВОК" => "Head",
            "СТЕБЕЛЬ" or "СТЕБЛЯ" or "СТЕБЛЕЙ" => "Stalk",
            "ЛОМТИК" or "ЛОМТИКА" or "ЛОМТИКОВ" => "Slice",
            "ЛИСТ" or "ЛИСТА" or "ЛИСТОВ" => "Sheet",
            "ГОРСТЬ" or "ГОРСТИ" => "Handful",
            "КАПЛЯ" or "КАПЛИ" or "КАПЕЛЬ" => "Drop",
            "БАНКА" or "БАНКИ" or "БАНОК" => "Can",
            "БУТЫЛКА" or "БУТЫЛКИ" or "БУТЫЛОК" => "Bottle",
            "ПАКЕТИК" or "ПАКЕТИКА" or "ПАКЕТИКОВ" => "Sachet",
            "КУБИК" or "КУБИКА" or "КУБИКОВ" => "Cube",
            "ЩЕПОТКА" or "ЩЕПОТКИ" => "Pinch",
            "УПАКОВКА" or "УПАКОВКИ" or "УПАКОВОК" => "Pack",
            "ПО ВКУСУ" => "ToTaste",
            _ => "Unknown"
        };
    }

    private static Section ClassifySection(string heading)
    {
        string normalized = heading.Trim();
        if (normalized.Contains("ингредиент", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("состав", StringComparison.OrdinalIgnoreCase))
        {
            return Section.Ingredients;
        }

        if (normalized.Contains("приготов", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("способ", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("рецепт", StringComparison.OrdinalIgnoreCase))
        {
            return Section.Steps;
        }

        return Section.None;
    }

    private static string ClassifyRecipeCategory(string title)
    {
        string normalized = title;
        if (normalized.Contains("суп", StringComparison.OrdinalIgnoreCase) || normalized.Contains("борщ", StringComparison.OrdinalIgnoreCase))
        {
            return "Soup";
        }

        if (normalized.Contains("салат", StringComparison.OrdinalIgnoreCase))
        {
            return "Salad";
        }

        if (normalized.Contains("напит", StringComparison.OrdinalIgnoreCase) || normalized.Contains("коктейл", StringComparison.OrdinalIgnoreCase))
        {
            return "Drink";
        }

        if (normalized.Contains("намазк", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("паштет", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("хумус", StringComparison.OrdinalIgnoreCase))
        {
            return "Spread";
        }

        if (normalized.Contains("соус", StringComparison.OrdinalIgnoreCase))
        {
            return "Sauce";
        }

        if (normalized.Contains("торт", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("печень", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("пирог", StringComparison.OrdinalIgnoreCase))
        {
            return "Baking";
        }

        return "MainCourse";
    }

    private enum Section
    {
        None,
        Ingredients,
        Steps
    }

    [GeneratedRegex(@"^=+\s*(.*?)\s*=+$", RegexOptions.CultureInvariant)]
    private static partial Regex HeadingRegex();

    [GeneratedRegex(@"^[*#:;]+\s*", RegexOptions.CultureInvariant)]
    private static partial Regex BulletRegex();

    [GeneratedRegex(@"^(?:[*#:;]+|\d+[.)])\s*", RegexOptions.CultureInvariant)]
    private static partial Regex BulletOrNumberRegex();

    [GeneratedRegex(@"^\s*(?:примерно\s+)?(?<amount>\d+(?:[.,]\d+)?(?:\s*[-–—]\s*\d+(?:[.,]\d+)?)?|\d+/\d+)?\s*(?<unit>г(?:р)?\.?|кг\.?|мл\.?|л\.?|шт\.?|ч\.\s*л\.?|ст\.\s*л\.?|дес\.\s*л\.?|стакан(?:а|ов)?|чаш(?:ка|ки|ек)|зубчик(?:а|ов)?|пуч(?:ок|ка|ков)|веточ(?:ка|ки|ек)|голов(?:ка|ки|ок)|стеб(?:ель|ля|лей)|ломтик(?:а|ов)?|лист(?:а|ов)?|горст(?:ь|и)|кап(?:ля|ли|ель)|банк(?:а|и|ок)|бутыл(?:ка|ки|ок)|пакетик(?:а|ов)?|кубик(?:а|ов)?|щепотк(?:а|и)|упаковк(?:а|и|ок)|по вкусу)?\s*(?<name>.+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex IngredientRegex();

    [GeneratedRegex(@"^(?<name>[^:]+):\s*(?<amount>\d+(?:[.,]\d+)?(?:\s*[-–—]\s*\d+(?:[.,]\d+)?)?|\d+/\d+)\s*(?<unit>г(?:р)?\.?|кг\.?|мл\.?|л\.?|шт\.?|ч\.\s*л\.?|ст\.\s*л\.?|дес\.\s*л\.?|стакан(?:а|ов)?|чаш(?:ка|ки|ек)|зубчик(?:а|ов)?|пуч(?:ок|ка|ков)|веточ(?:ка|ки|ек)|голов(?:ка|ки|ок)|стеб(?:ель|ля|лей)|ломтик(?:а|ов)?|лист(?:а|ов)?|горст(?:ь|и)|кап(?:ля|ли|ель)|банк(?:а|и|ок)|бутыл(?:ка|ки|ок)|пакетик(?:а|ов)?|кубик(?:а|ов)?|щепотк(?:а|и)|упаковк(?:а|и|ок))(?<suffix>.*)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex NameFirstIngredientRegex();

    [GeneratedRegex(@"\s*[-–—]\s*", RegexOptions.CultureInvariant)]
    private static partial Regex RangeSeparatorRegex();

    [GeneratedRegex(@"[-–—]", RegexOptions.CultureInvariant)]
    private static partial Regex RangeAmountRegex();

    [GeneratedRegex(@"\s*[,;]\s*", RegexOptions.CultureInvariant)]
    private static partial Regex InlineIngredientSeparatorRegex();

    [GeneratedRegex(@"^Рецепт:\s*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex RecipeNamespacePrefixRegex();
}
