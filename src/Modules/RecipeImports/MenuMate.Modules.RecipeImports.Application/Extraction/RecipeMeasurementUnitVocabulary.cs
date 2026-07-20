using System.Globalization;
using System.Text.RegularExpressions;

namespace MenuMate.Modules.RecipeImports.Application.Extraction;

/// <summary>
/// Единый словарь единиц измерения, используемый в промпте и серверной проверке импорта.
/// </summary>
internal static partial class RecipeMeasurementUnitVocabulary
{
    private const string NumberPattern = @"(?:(?:\d+\s+)?\d+\s*/\s*\d+|\d+(?:[.,]\d+)?)";

    public static IReadOnlyList<RecipeMeasurementUnitDescriptor> All { get; } =
    [
        Unit("Gram", "г", isPrecise: true, defaultAmountOne: false,
            "г", "г.", "гр", "гр.", "грамм", "грамма", "граммов"),
        Unit("Kilogram", "кг", isPrecise: true, defaultAmountOne: false,
            "кг", "кг.", "килограмм", "килограмма", "килограммов"),
        Unit("Milliliter", "мл", isPrecise: true, defaultAmountOne: false,
            "мл", "мл.", "миллилитр", "миллилитра", "миллилитров"),
        Unit("Liter", "л", isPrecise: true, defaultAmountOne: false,
            "л", "л.", "литр", "литра", "литров"),
        Unit("Piece", "шт.", isPrecise: false, defaultAmountOne: false,
            "шт", "шт.", "штука", "штуки", "штук"),
        Unit("Teaspoon", "ч. л.", isPrecise: false, defaultAmountOne: true,
            "ч. л.", "ч.л.", "ч л", "чайная ложка", "чайные ложки", "чайных ложек"),
        Unit("Tablespoon", "ст. л.", isPrecise: false, defaultAmountOne: true,
            "ст. л.", "ст.л.", "ст л", "столовая ложка", "столовые ложки", "столовых ложек"),
        Unit("Pinch", "щепотка", isPrecise: false, defaultAmountOne: true,
            "щепотка", "щепотки", "щепоток"),
        Unit("Pack", "упаковка", isPrecise: false, defaultAmountOne: true,
            "уп", "уп.", "упаковка", "упаковки", "упаковок"),
        Unit("ToTaste", "по вкусу", isPrecise: false, defaultAmountOne: false,
            "по вкусу"),
        Unit("Unknown", "без единицы", isPrecise: false, defaultAmountOne: false),
        Unit("Glass", "стакан", isPrecise: false, defaultAmountOne: true,
            "стакан", "стакана", "стаканов"),
        Unit("Cup", "чашка", isPrecise: false, defaultAmountOne: true,
            "чашка", "чашки", "чашек"),
        Unit("Dessertspoon", "дес. л.", isPrecise: false, defaultAmountOne: true,
            "дес. л.", "дес.л.", "дес л", "десертная ложка", "десертные ложки", "десертных ложек"),
        Unit("Clove", "зубчик", isPrecise: false, defaultAmountOne: true,
            "зуб", "зуб.", "зубчик", "зубчика", "зубчиков"),
        Unit("Bunch", "пучок", isPrecise: false, defaultAmountOne: true,
            "пучок", "пучка", "пучков"),
        Unit("Sprig", "веточка", isPrecise: false, defaultAmountOne: true,
            "веточка", "веточки", "веточек"),
        Unit("Head", "головка", isPrecise: false, defaultAmountOne: true,
            "головка", "головки", "головок"),
        Unit("Stalk", "стебель", isPrecise: false, defaultAmountOne: true,
            "стебель", "стебля", "стеблей"),
        Unit("Slice", "ломтик", isPrecise: false, defaultAmountOne: true,
            "ломтик", "ломтика", "ломтиков"),
        Unit("Sheet", "лист", isPrecise: false, defaultAmountOne: true,
            "лист", "листа", "листов"),
        Unit("Handful", "горсть", isPrecise: false, defaultAmountOne: true,
            "горсть", "горсти", "горстей"),
        Unit("Drop", "капля", isPrecise: false, defaultAmountOne: true,
            "капля", "капли", "капель"),
        Unit("Can", "банка", isPrecise: false, defaultAmountOne: true,
            "банка", "банки", "банок"),
        Unit("Jar", "баночка", isPrecise: false, defaultAmountOne: true,
            "баночка", "баночки", "баночек"),
        Unit("Bottle", "бутылка", isPrecise: false, defaultAmountOne: true,
            "бутылка", "бутылки", "бутылок"),
        Unit("Sachet", "пакетик", isPrecise: false, defaultAmountOne: true,
            "пакетик", "пакетика", "пакетиков"),
        Unit("Cube", "кубик", isPrecise: false, defaultAmountOne: true,
            "кубик", "кубика", "кубиков")
    ];

    public static string CreatePromptReference() =>
        string.Join(
            '\n',
            All.Select(unit => unit.Aliases.Count > 0
                ? $"- {unit.Value}: {string.Join(", ", unit.Aliases)}"
                : $"- {unit.Value}: числовое количество без распознанной единицы"));

    public static RecipeSourceQuantity? ParseQuantity(string? sourceText)
    {
        if (string.IsNullOrWhiteSpace(sourceText))
        {
            return null;
        }

        List<QuantityCandidate> candidates = [];
        foreach (RecipeMeasurementUnitDescriptor unit in All.Where(unit => unit.IsMeasured))
        {
            Match match = unit.QuantityRegex!.Match(sourceText);
            if (!match.Success || !TryParseDecimal(match.Groups["min"].Value, out decimal minimum))
            {
                continue;
            }

            decimal? maximum = TryParseDecimal(match.Groups["max"].Value, out decimal parsedMaximum)
                ? parsedMaximum
                : null;
            candidates.Add(new QuantityCandidate(unit, minimum, maximum, match.Index, match.Length));
        }

        QuantityCandidate? selected = candidates
            .OrderByDescending(candidate => candidate.Descriptor.IsPrecise)
            .ThenBy(candidate => candidate.Index)
            .FirstOrDefault();

        Match implicitRange = ImplicitRangeRegex().Matches(sourceText)
            .FirstOrDefault(match => candidates.All(candidate => !Overlaps(match, candidate)))
            ?? Match.Empty;

        if (selected is not null)
        {
            List<string> rangeComments = [];
            if (selected.Maximum is { } selectedMaximum)
            {
                rangeComments.Add(FormatRange(selected.Minimum, selectedMaximum, selected.Descriptor.DisplayName));
            }

            QuantityCandidate? secondaryRange = candidates.FirstOrDefault(candidate =>
                candidate.Maximum is not null && candidate != selected);
            if (secondaryRange?.Maximum is { } secondaryMaximum)
            {
                rangeComments.Add(FormatRange(
                    secondaryRange.Minimum,
                    secondaryMaximum,
                    secondaryRange.Descriptor.DisplayName));
            }
            else if (implicitRange.Success &&
                     TryParseDecimal(implicitRange.Groups["min"].Value, out decimal implicitMinimum) &&
                     TryParseDecimal(implicitRange.Groups["max"].Value, out decimal implicitMaximum))
            {
                rangeComments.Add(FormatRange(implicitMinimum, implicitMaximum, "шт."));
            }

            return new RecipeSourceQuantity(
                selected.Minimum,
                selected.Descriptor.Value,
                rangeComments);
        }

        if (implicitRange.Success &&
            TryParseDecimal(implicitRange.Groups["min"].Value, out decimal rangeMinimum) &&
            TryParseDecimal(implicitRange.Groups["max"].Value, out decimal rangeMaximum))
        {
            return new RecipeSourceQuantity(
                rangeMinimum,
                "Piece",
                [FormatRange(rangeMinimum, rangeMaximum, "шт.")]);
        }

        RecipeMeasurementUnitDescriptor? unitWithoutNumber = All
            .Where(unit => unit.DefaultAmountOne && unit.UnitRegex!.IsMatch(sourceText))
            .OrderBy(unit => unit.UnitRegex!.Match(sourceText).Index)
            .FirstOrDefault();
        if (unitWithoutNumber is not null)
        {
            return new RecipeSourceQuantity(1m, unitWithoutNumber.Value, []);
        }

        RecipeMeasurementUnitDescriptor toTaste = All.Single(unit => unit.Value == "ToTaste");
        return toTaste.UnitRegex!.IsMatch(sourceText)
            ? new RecipeSourceQuantity(null, "ToTaste", [])
            : null;
    }

    public static bool ShouldDefaultToOne(string unit) =>
        All.FirstOrDefault(candidate => candidate.Value.Equals(unit, StringComparison.OrdinalIgnoreCase))
            ?.DefaultAmountOne == true;

    public static bool IsUnitOnlyText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        foreach (RecipeMeasurementUnitDescriptor unit in All.Where(unit => unit.UnitRegex is not null))
        {
            string remaining = unit.UnitRegex!
                .Replace(value, string.Empty)
                .Trim(' ', ',', ';', '.', '-', '–', '—', '(', ')');
            if (remaining.Length == 0)
            {
                return true;
            }
        }

        return false;
    }

    private static RecipeMeasurementUnitDescriptor Unit(
        string value,
        string displayName,
        bool isPrecise,
        bool defaultAmountOne,
        params string[] aliases) =>
        new(value, displayName, isPrecise, defaultAmountOne, aliases);

    private static bool TryParseDecimal(string value, out decimal result)
    {
        Match fraction = FractionRegex().Match(value);
        if (fraction.Success &&
            decimal.TryParse(
                fraction.Groups["numerator"].Value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out decimal numerator) &&
            decimal.TryParse(
                fraction.Groups["denominator"].Value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out decimal denominator) &&
            denominator != 0)
        {
            decimal whole = decimal.TryParse(
                fraction.Groups["whole"].Value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out decimal parsedWhole)
                ? parsedWhole
                : 0m;
            result = Math.Round(whole + numerator / denominator, 2, MidpointRounding.AwayFromZero);
            return true;
        }

        return decimal.TryParse(
            value.Replace(',', '.'),
            NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out result);
    }

    private static string FormatRange(decimal minimum, decimal maximum, string unit) =>
        $"{minimum.ToString(CultureInfo.InvariantCulture)}–{maximum.ToString(CultureInfo.InvariantCulture)} {unit}";

    private static bool Overlaps(Match match, QuantityCandidate candidate) =>
        match.Index < candidate.Index + candidate.Length && candidate.Index < match.Index + match.Length;

    [GeneratedRegex(
        @"(?<![\d.,/])(?<min>(?:(?:\d+\s+)?\d+\s*/\s*\d+|\d+(?:[.,]\d+)?))\s*[-–—]\s*(?<max>(?:(?:\d+\s+)?\d+\s*/\s*\d+|\d+(?:[.,]\d+)?))(?!\s*%)",
        RegexOptions.CultureInvariant)]
    private static partial Regex ImplicitRangeRegex();

    [GeneratedRegex(
        @"^\s*(?:(?<whole>\d+)\s+)?(?<numerator>\d+)\s*/\s*(?<denominator>\d+)\s*$",
        RegexOptions.CultureInvariant)]
    private static partial Regex FractionRegex();

    private sealed record QuantityCandidate(
        RecipeMeasurementUnitDescriptor Descriptor,
        decimal Minimum,
        decimal? Maximum,
        int Index,
        int Length);

    internal sealed class RecipeMeasurementUnitDescriptor
    {
        public RecipeMeasurementUnitDescriptor(
            string value,
            string displayName,
            bool isPrecise,
            bool defaultAmountOne,
            IReadOnlyList<string> aliases)
        {
            Value = value;
            DisplayName = displayName;
            IsPrecise = isPrecise;
            DefaultAmountOne = defaultAmountOne;
            Aliases = aliases;

            if (aliases.Count == 0)
            {
                return;
            }

            string aliasPattern = string.Join(
                '|',
                aliases
                    .OrderByDescending(alias => alias.Length)
                    .Select(alias => Regex.Escape(alias).Replace(@"\ ", @"\s*", StringComparison.Ordinal)));
            string boundedAliasPattern = $@"(?<![\p{{L}}\p{{N}}])(?:{aliasPattern})(?![\p{{L}}\p{{N}}])";
            UnitRegex = new Regex(
                boundedAliasPattern,
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
                TimeSpan.FromSeconds(1));
            QuantityRegex = new Regex(
                $@"(?<min>{NumberPattern})(?:\s*[-–—]\s*(?<max>{NumberPattern}))?\s*{boundedAliasPattern}",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
                TimeSpan.FromSeconds(1));
        }

        public string Value { get; }

        public string DisplayName { get; }

        public bool IsPrecise { get; }

        public bool DefaultAmountOne { get; }

        public IReadOnlyList<string> Aliases { get; }

        public bool IsMeasured => Value is not ("ToTaste" or "Unknown");

        public Regex? UnitRegex { get; }

        public Regex? QuantityRegex { get; }
    }
}

internal sealed record RecipeSourceQuantity(
    decimal? Amount,
    string Unit,
    IReadOnlyCollection<string> RangeComments);
