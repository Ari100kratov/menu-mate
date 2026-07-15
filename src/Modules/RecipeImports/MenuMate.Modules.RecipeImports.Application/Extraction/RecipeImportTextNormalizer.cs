using System.Text.RegularExpressions;
using MenuMate.Contracts.Recipes;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.RecipeImports.Application.Extraction;

/// <summary>
/// Нормализует текст, полученный при распознавании рецепта.
/// </summary>
public static partial class RecipeImportTextNormalizer
{
    /// <summary>
    /// Нормализует ингредиенты, шаги и теги, сверяя количества с исходными строками при их наличии.
    /// </summary>
    public static CreateRecipeRequest Normalize(
        CreateRecipeRequest recipe,
        IReadOnlyList<string>? ingredientSourceTexts = null)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        var normalizedIngredients = new List<RecipeIngredientRequest>(recipe.Ingredients.Count);
        var seenIngredientSources = new HashSet<IngredientSourceKey>();
        var seenIngredientRanges = new HashSet<IngredientRangeKey>();
        int ingredientIndex = 0;
        foreach (RecipeIngredientRequest ingredient in recipe.Ingredients)
        {
            string? sourceText = ingredientSourceTexts is not null && ingredientIndex < ingredientSourceTexts.Count
                ? ingredientSourceTexts[ingredientIndex]
                : null;
            normalizedIngredients.AddRange(NormalizeIngredient(ingredient, sourceText)
                .Where(normalizedIngredient => ShouldKeepIngredient(
                    normalizedIngredient,
                    sourceText,
                    ingredientSourceTexts is not null,
                    seenIngredientSources,
                    seenIngredientRanges)));
            ingredientIndex++;
        }

        return recipe with
        {
            Ingredients = normalizedIngredients,
            Steps = recipe.Steps
                .Select(step => new PreparationStepRequest(StepPrefixRegex().Replace(step.Text, string.Empty).Trim()))
                .ToArray(),
            Tags = NormalizeTags(recipe.Tags, recipe.Title)
        };
    }

    private static bool ShouldKeepIngredient(
        RecipeIngredientRequest ingredient,
        string? sourceText,
        bool hasSourceEvidence,
        HashSet<IngredientSourceKey> seenIngredientSources,
        HashSet<IngredientRangeKey> seenIngredientRanges)
    {
        string normalizedProductName = ProductNameNormalizer.NormalizeForComparison(ingredient.ProductName);
        if (!string.IsNullOrWhiteSpace(sourceText))
        {
            var sourceKey = new IngredientSourceKey(
                normalizedProductName,
                NormalizeSourceTextForComparison(sourceText));
            if (!seenIngredientSources.Add(sourceKey))
            {
                return false;
            }
        }

        if (!hasSourceEvidence ||
            string.IsNullOrWhiteSpace(ingredient.Comment) ||
            !RangeValueRegex().IsMatch(ingredient.Comment))
        {
            return true;
        }

        var rangeKey = new IngredientRangeKey(
            normalizedProductName,
            ingredient.Amount,
            ingredient.Unit,
            TextNormalizer.NormalizeSearchText(ingredient.Comment));
        return seenIngredientRanges.Add(rangeKey);
    }

    private static string NormalizeSourceTextForComparison(string sourceText) =>
        TextNormalizer.NormalizeSearchText(sourceText)
            .Replace('Ё', 'Е')
            .Replace('–', '-')
            .Replace('—', '-');

    private static IReadOnlyCollection<RecipeIngredientRequest> NormalizeIngredient(
        RecipeIngredientRequest ingredient,
        string? sourceText)
    {
        RecipeIngredientRequest normalizedIngredient = ingredient with
        {
            ProductName = ProductNameNormalizer.Normalize(ingredient.ProductName)
        };

        normalizedIngredient = NormalizeProductCharacteristic(normalizedIngredient, sourceText);

        RecipeSourceQuantity? sourceQuantity = RecipeMeasurementUnitVocabulary.ParseQuantity(sourceText);
        sourceQuantity ??= normalizedIngredient.Amount is null
            ? RecipeMeasurementUnitVocabulary.ParseQuantity(normalizedIngredient.Comment)
            : null;

        if (sourceQuantity is not null)
        {
            normalizedIngredient = normalizedIngredient with
            {
                Amount = sourceQuantity.Amount,
                Unit = sourceQuantity.Unit,
                Comment = MergeQuantityComments(normalizedIngredient.Comment, sourceQuantity.RangeComments)
            };
        }
        else if (normalizedIngredient.Amount is null &&
                 RecipeMeasurementUnitVocabulary.ShouldDefaultToOne(normalizedIngredient.Unit))
        {
            normalizedIngredient = normalizedIngredient with { Amount = 1m };
        }
        else if (normalizedIngredient.Amount is null)
        {
            normalizedIngredient = normalizedIngredient with { Unit = "ToTaste" };
        }

        Match combinedMatch = CombinedSaltAndPepperRegex().Match(normalizedIngredient.ProductName);
        if (!combinedMatch.Success)
        {
            return [normalizedIngredient];
        }

        return
        [
            normalizedIngredient with
            {
                ProductName = "соль",
                Category = "Spices"
            },
            normalizedIngredient with
            {
                ProductName = ProductNameNormalizer.Normalize(combinedMatch.Groups["pepper"].Value),
                Category = "Spices"
            }
        ];
    }

    private static RecipeIngredientRequest NormalizeProductCharacteristic(
        RecipeIngredientRequest ingredient,
        string? sourceText)
    {
        Match percentage = PercentageRegex().Match(sourceText ?? string.Empty);
        if (!percentage.Success)
        {
            percentage = PercentageRegex().Match(ingredient.Comment ?? string.Empty);
        }

        if (!percentage.Success || !PercentageProductRegex().IsMatch(ingredient.ProductName))
        {
            return ingredient;
        }

        string percentageText = $"{percentage.Groups["value"].Value.Replace(',', '.')}%";
        string productName = PercentageRegex().IsMatch(ingredient.ProductName)
            ? ingredient.ProductName
            : $"{ingredient.ProductName} {percentageText}";
        string? comment = RemovePercentageCharacteristic(ingredient.Comment);

        return ingredient with
        {
            ProductName = ProductNameNormalizer.Normalize(productName),
            Comment = comment
        };
    }

    private static string? RemovePercentageCharacteristic(string? comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            return null;
        }

        string result = PercentageCharacteristicRegex()
            .Replace(comment, string.Empty)
            .Trim(' ', ',', ';', '.', '-', '–', '—', '(', ')');
        return string.IsNullOrWhiteSpace(result) ? null : result;
    }

    private static string? MergeQuantityComments(
        string? comment,
        IReadOnlyCollection<string> quantityComments)
    {
        string? normalizedComment = string.IsNullOrWhiteSpace(comment)
            ? null
            : comment.Trim();
        if (normalizedComment is not null &&
            (QuantityOnlyCommentRegex().IsMatch(normalizedComment) ||
             RecipeMeasurementUnitVocabulary.IsUnitOnlyText(normalizedComment)))
        {
            normalizedComment = null;
        }

        List<string> parts = normalizedComment is null ? [] : [normalizedComment];
        parts.AddRange(quantityComments
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(quantityComment => parts.All(part =>
                !part.Equals(quantityComment, StringComparison.OrdinalIgnoreCase))));

        return parts.Count == 0 ? null : string.Join("; ", parts);
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

    private readonly record struct IngredientSourceKey(string ProductName, string SourceText);

    private readonly record struct IngredientRangeKey(
        string ProductName,
        decimal? Amount,
        string Unit,
        string Comment);

    [GeneratedRegex(
        @"^\s*(?:(?:(?:шаг|step)\s*)?\d+\s*[.)\]:\-–—]\s*|(?:шаг|step)\s*\d+\s+)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex StepPrefixRegex();

    [GeneratedRegex(
        @"(?<value>\d+(?:[.,]\d+)?)\s*%",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PercentageRegex();

    [GeneratedRegex(
        @"(?:сливк|молок|сметан|кефир|ряжен|йогурт|творог|сыр|масл|майонез|уксус|шоколад|какао)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PercentageProductRegex();

    [GeneratedRegex(
        @"(?:жирност(?:ь|и|ью)\s*)?\d+(?:[.,]\d+)?\s*%",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PercentageCharacteristicRegex();

    [GeneratedRegex(
        @"^\s*\d+(?:[.,]\d+)?\s*[-–—]\s*\d+(?:[.,]\d+)?(?:\s*[\p{L}.]+(?:\s*[\p{L}.]+)*)?\s*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex QuantityOnlyCommentRegex();

    [GeneratedRegex(
        @"\d+(?:[.,]\d+)?\s*[-–—]\s*\d+(?:[.,]\d+)?",
        RegexOptions.CultureInvariant)]
    private static partial Regex RangeValueRegex();

    [GeneratedRegex(
        @"^соль\s*(?:,|и|/|\+)\s*(?<pepper>(?:(?:черный|белый|душистый|молотый)\s+){0,2}перец)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex CombinedSaltAndPepperRegex();
}
