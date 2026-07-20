using MenuMate.Contracts.Recipes;
using MenuMate.Modules.RecipeImports.Application.Extraction;
using MenuMate.Modules.Recipes.Domain.Enums;

namespace MenuMate.Modules.RecipeImports.Application.UnitTests.Extraction;

public sealed class RecipeImportTextNormalizerTests
{
    [Fact]
    public void MeasurementUnitVocabularyShouldMatchDomainEnum()
    {
        Assert.Equal(
            Enum.GetNames<MeasurementUnit>(),
            RecipeMeasurementUnitVocabulary.All.Select(unit => unit.Value));
    }

    [Fact]
    public void NormalizeShouldRemoveOnlyOrdinalPrefixesFromSteps()
    {
        var recipe = new CreateRecipeRequest(
            "Суп",
            null,
            2,
            "Soup",
            "Public",
            null,
            null,
            null,
            [],
            [
                new("1. Нарезать овощи"),
                new("Шаг 2: Добавить воду"),
                new("3 минуты варить")
            ],
            []);

        CreateRecipeRequest normalized = RecipeImportTextNormalizer.Normalize(recipe);

        Assert.Collection(
            normalized.Steps,
            step => Assert.Equal("Нарезать овощи", step.Text),
            step => Assert.Equal("Добавить воду", step.Text),
            step => Assert.Equal("3 минуты варить", step.Text));
    }

    [Fact]
    public void NormalizeShouldSetToTasteWhenIngredientAmountIsMissing()
    {
        var recipe = new CreateRecipeRequest(
            "Суп",
            null,
            2,
            "Soup",
            "Public",
            null,
            null,
            null,
            [
                new(null, "Соль", null, "Unknown", "Spices", null, false),
                new(null, "Масло", 20m, "Milliliter", "OilsAndSauces", null, false)
            ],
            [],
            []);

        CreateRecipeRequest normalized = RecipeImportTextNormalizer.Normalize(recipe);

        Assert.Collection(
            normalized.Ingredients,
            ingredient =>
            {
                Assert.Null(ingredient.Amount);
                Assert.Equal("ToTaste", ingredient.Unit);
            },
            ingredient => Assert.Equal("Milliliter", ingredient.Unit));
    }

    [Fact]
    public void NormalizeShouldDiscardModelDefaultOneWhenSourceHasNoQuantityOrUnit()
    {
        CreateRecipeRequest recipe = CreateRecipe(
            [new(null, "Соль", 1m, "Unknown", "Spices", null, false)]);

        CreateRecipeRequest normalized = RecipeImportTextNormalizer.Normalize(recipe, ["Соль"]);

        RecipeIngredientRequest ingredient = Assert.Single(normalized.Ingredients);
        Assert.Null(ingredient.Amount);
        Assert.Equal("ToTaste", ingredient.Unit);
    }

    [Fact]
    public void NormalizeShouldKeepUnknownUnitAmountWhenSourceContainsANumber()
    {
        CreateRecipeRequest recipe = CreateRecipe(
            [new(null, "Сахар", 100m, "Unknown", "Other", null, false)]);

        CreateRecipeRequest normalized = RecipeImportTextNormalizer.Normalize(recipe, ["Сахар — 100"]);

        RecipeIngredientRequest ingredient = Assert.Single(normalized.Ingredients);
        Assert.Equal(100m, ingredient.Amount);
        Assert.Equal("Unknown", ingredient.Unit);
    }

    [Fact]
    public void NormalizeShouldUsePinchAsIngredientQuantityWhenItIsTheOnlyComment()
    {
        var recipe = new CreateRecipeRequest(
            "Суп",
            null,
            2,
            "Soup",
            "Public",
            null,
            null,
            null,
            [new(null, "Соль", null, "Unknown", "Spices", "щепотка", false)],
            [],
            []);

        CreateRecipeRequest normalized = RecipeImportTextNormalizer.Normalize(recipe);

        RecipeIngredientRequest ingredient = Assert.Single(normalized.Ingredients);
        Assert.Equal(1m, ingredient.Amount);
        Assert.Equal("Pinch", ingredient.Unit);
        Assert.Null(ingredient.Comment);
    }

    [Theory]
    [InlineData("Петрушка — пучок", "Петрушка", null, "ToTaste", 1, "Bunch")]
    [InlineData("Вода — 1 стакан", "Вода", null, "Cup", 1, "Glass")]
    [InlineData("Чеснок — 2 зуб.", "Чеснок", null, "ToTaste", 2, "Clove")]
    public void NormalizeShouldRestoreExactUnitsFromIngredientSourceText(
        string sourceText,
        string productName,
        decimal? modelAmount,
        string modelUnit,
        decimal expectedAmount,
        string expectedUnit)
    {
        CreateRecipeRequest recipe = CreateRecipe(
            [new(null, productName, modelAmount, modelUnit, "Other", null, false)]);

        CreateRecipeRequest normalized = RecipeImportTextNormalizer.Normalize(recipe, [sourceText]);

        RecipeIngredientRequest ingredient = Assert.Single(normalized.Ingredients);
        Assert.Equal(expectedAmount, ingredient.Amount);
        Assert.Equal(expectedUnit, ingredient.Unit);
        Assert.Null(ingredient.Comment);
    }

    [Theory]
    [InlineData("Соль — 1/3 ст. л.", 33)]
    [InlineData("Соль — 2/3 ст. л.", 67)]
    [InlineData("Соль — 1 1/2 ст. л.", 150)]
    public void NormalizeShouldConvertFractionsInsteadOfUsingTheirDenominator(
        string sourceText,
        int expectedHundredths)
    {
        CreateRecipeRequest recipe = CreateRecipe(
            [new(null, "Соль", 3m, "Tablespoon", "Spices", null, false)]);

        CreateRecipeRequest normalized = RecipeImportTextNormalizer.Normalize(recipe, [sourceText]);

        RecipeIngredientRequest ingredient = Assert.Single(normalized.Ingredients);
        Assert.Equal(expectedHundredths / 100m, ingredient.Amount);
        Assert.Equal("Tablespoon", ingredient.Unit);
    }

    [Fact]
    public void NormalizeShouldKeepPieceRangeInsteadOfChangingItToTaste()
    {
        CreateRecipeRequest recipe = CreateRecipe(
            [new(null, "Персики", null, "ToTaste", "Produce", "2-3", false)]);

        CreateRecipeRequest normalized = RecipeImportTextNormalizer.Normalize(
            recipe,
            ["Персики — 2-3"]);

        RecipeIngredientRequest ingredient = Assert.Single(normalized.Ingredients);
        Assert.Equal(2m, ingredient.Amount);
        Assert.Equal("Piece", ingredient.Unit);
        Assert.Equal("2–3 шт.", ingredient.Comment);
    }

    [Fact]
    public void NormalizeShouldCollapseDuplicateRangeProductsFromSameSourceLine()
    {
        CreateRecipeRequest recipe = CreateRecipe(
        [
            new(null, "Болгарский перец", 1m, "Piece", "Produce", "1–2 шт.", false),
            new(null, "Болгарский перец", 1m, "Piece", "Produce", "1–2 шт.", false)
        ]);

        CreateRecipeRequest normalized = RecipeImportTextNormalizer.Normalize(
            recipe,
            ["1-2 болгарских перца", "болгарский перец: 1–2 штуки"]);

        RecipeIngredientRequest ingredient = Assert.Single(normalized.Ingredients);
        Assert.Equal("болгарский перец", ingredient.ProductName);
        Assert.Equal(1m, ingredient.Amount);
        Assert.Equal("Piece", ingredient.Unit);
        Assert.Equal("1–2 шт.", ingredient.Comment);
    }

    [Fact]
    public void NormalizeShouldKeepDifferentProductsFromSameSourceLine()
    {
        CreateRecipeRequest recipe = CreateRecipe(
        [
            new(null, "Соль", null, "ToTaste", "Spices", null, false),
            new(null, "Черный перец", null, "ToTaste", "Spices", null, false)
        ]);

        CreateRecipeRequest normalized = RecipeImportTextNormalizer.Normalize(
            recipe,
            ["Соль и черный перец — по вкусу", "Соль и черный перец — по вкусу"]);

        Assert.Collection(
            normalized.Ingredients,
            ingredient => Assert.Equal("соль", ingredient.ProductName),
            ingredient => Assert.Equal("черный перец", ingredient.ProductName));
    }

    [Fact]
    public void NormalizeShouldRemoveUnitDuplicatedInComment()
    {
        CreateRecipeRequest recipe = CreateRecipe(
            [new(null, "Петрушка", null, "ToTaste", "HerbsAndGreens", "пучок", false)]);

        CreateRecipeRequest normalized = RecipeImportTextNormalizer.Normalize(
            recipe,
            ["Петрушка — пучок"]);

        RecipeIngredientRequest ingredient = Assert.Single(normalized.Ingredients);
        Assert.Equal(1m, ingredient.Amount);
        Assert.Equal("Bunch", ingredient.Unit);
        Assert.Null(ingredient.Comment);
    }

    [Fact]
    public void NormalizeShouldPreferExactWeightAndKeepPieceRangeInComment()
    {
        CreateRecipeRequest recipe = CreateRecipe(
            [new(null, "Кабачки", 2m, "Piece", "Produce", null, false)]);

        CreateRecipeRequest normalized = RecipeImportTextNormalizer.Normalize(
            recipe,
            ["Кабачки — 2-3 шт. (380 г.)"]);

        RecipeIngredientRequest ingredient = Assert.Single(normalized.Ingredients);
        Assert.Equal(380m, ingredient.Amount);
        Assert.Equal("Gram", ingredient.Unit);
        Assert.Equal("2–3 шт.", ingredient.Comment);
    }

    [Fact]
    public void NormalizeShouldKeepPercentageInProductNameAndNotUseItAsAmount()
    {
        CreateRecipeRequest recipe = CreateRecipe(
            [new(null, "Сливки", 33m, "Gram", "Dairy", "жирность 33%", false)]);

        CreateRecipeRequest normalized = RecipeImportTextNormalizer.Normalize(
            recipe,
            ["Сливки 33% — 80 г"]);

        RecipeIngredientRequest ingredient = Assert.Single(normalized.Ingredients);
        Assert.Equal("сливки 33%", ingredient.ProductName);
        Assert.Equal(80m, ingredient.Amount);
        Assert.Equal("Gram", ingredient.Unit);
        Assert.Null(ingredient.Comment);
    }

    [Fact]
    public void NormalizeShouldSplitCombinedSaltAndPepper()
    {
        CreateRecipeRequest recipe = CreateRecipe(
            [new(null, "Соль и черный перец", null, "ToTaste", "Spices", null, false)]);

        CreateRecipeRequest normalized = RecipeImportTextNormalizer.Normalize(
            recipe,
            ["Соль и черный перец — по вкусу"]);

        Assert.Collection(
            normalized.Ingredients,
            ingredient =>
            {
                Assert.Equal("соль", ingredient.ProductName);
                Assert.Equal("ToTaste", ingredient.Unit);
            },
            ingredient =>
            {
                Assert.Equal("черный перец", ingredient.ProductName);
                Assert.Equal("ToTaste", ingredient.Unit);
            });
    }

    [Fact]
    public void NormalizeShouldNormalizeIngredientNamesAndDiscardRecipeTypeTags()
    {
        var recipe = new CreateRecipeRequest(
            "Овощной суп",
            null,
            2,
            "Soup",
            "Public",
            null,
            null,
            null,
            [new(null, "  Свёкла  ", 1m, "Piece", "Produce", null, false)],
            [],
            [
                "Суп", "Обед", "Азиатская кухня", "Вегетарианское", "Быстро",
                "Сезонное", "Домашнее", "Праздничное", "Лёгкое"
            ]);

        CreateRecipeRequest normalized = RecipeImportTextNormalizer.Normalize(recipe);

        Assert.Equal("свекла", Assert.Single(normalized.Ingredients).ProductName);
        Assert.Equal(
            ["азиатская кухня", "вегетарианское", "быстро", "сезонное", "домашнее", "праздничное"],
            normalized.Tags);
    }

    [Fact]
    public void NormalizeWarningsShouldReplaceTechnicalDetailsAndKeepUserFriendlyWarnings()
    {
        IReadOnlyCollection<string> normalized = RecipeImportWarningNormalizer.Normalize(
        [
            "1. Проверьте количество соли: текст на изображении размыт.",
            "Количество порций рассчитано приблизительно — проверьте его.",
            "recipe.ingredients[0].amount is null because confidence=0.42",
            "JSON schema returned enum Other",
            "  "
        ]);

        Assert.Collection(
            normalized,
            warning => Assert.Equal("Проверьте количество соли: текст на изображении размыт.", warning),
            warning => Assert.Equal(
                "Количество порций рассчитано приблизительно — проверьте его.",
                warning),
            warning => Assert.Equal(
                "Проверьте распознанные данные: часть информации на изображении прочитана неуверенно.",
                warning));
    }

    private static CreateRecipeRequest CreateRecipe(IReadOnlyCollection<RecipeIngredientRequest> ingredients) =>
        new(
            "Тестовый рецепт",
            null,
            2,
            "Other",
            "Public",
            null,
            null,
            null,
            ingredients,
            [],
            []);
}
