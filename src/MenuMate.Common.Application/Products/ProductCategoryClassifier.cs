namespace MenuMate.Common.Application.Products;

/// <summary>
/// Classifies ingredient and product names into the shared product categories.
/// </summary>
public static class ProductCategoryClassifier
{
    private static readonly (string Category, string[] Keywords)[] Rules =
    [
        ("Eggs", ["яйц"]),
        ("Dairy", ["молок", "сливк", "сметан", "кефир", "йогурт", "творог", "сыр", "масло сливоч"]),
        ("MeatAndPoultry", ["мяс", "говя", "свин", "баран", "кур", "индей", "утк", "фарш", "бекон", "колбас", "ветчин"]),
        ("FishAndSeafood", ["рыб", "лосос", "семг", "форел", "треск", "сельд", "кревет", "кальмар", "мид", "краб"]),
        ("Legumes", ["фасол", "горох", "нут", "чечев", "боб"]),
        ("NutsAndSeeds", ["орех", "миндал", "фисташ", "арахис", "семеч", "кунжут", "семена"]),
        ("OilsAndSauces", ["масло раст", "масло олив", "соус", "майонез", "кетчуп", "горчиц", "уксус"]),
        ("CannedAndPreserved", ["консерв", "маринован", "солен", "варенье", "джем"]),
        ("SweetsAndConfectionery", ["шоколад", "конфет", "мармелад", "зефир", "мед", "сироп"]),
        ("HerbsAndGreens", ["укроп", "петруш", "кинз", "базилик", "розмарин", "тимьян", "мята", "зелень", "шпинат"]),
        ("Spices", ["соль", "перец", "паприк", "кориандр", "куркум", "корица", "ванил", "специ", "приправа"]),
        ("GrainsAndPasta", ["рис", "греч", "круп", "макарон", "спагет", "лапш", "овся", "мука"]),
        ("Bakery", ["хлеб", "батон", "булк", "лаваш", "сухар"]),
        ("Drinks", ["сок", "вода", "чай", "кофе", "вино", "пиво", "квас"]),
        ("Frozen", ["заморож"]),
        ("Produce", ["лук", "чеснок", "морков", "картоф", "томат", "помидор", "огур", "капуст", "свекл", "перец", "яблок", "груш", "лимон", "апельс", "ягод", "гриб"])
    ];

    /// <summary>
    /// Returns the best matching shared product category for a product or ingredient name.
    /// </summary>
    public static string GetCategory(string productName)
    {
        ArgumentNullException.ThrowIfNull(productName);

        string normalized = productName.Trim();
        foreach ((string category, string[] keywords) in Rules)
        {
            if (keywords.Any(keyword => normalized.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            {
                return category;
            }
        }

        return "Grocery";
    }
}
