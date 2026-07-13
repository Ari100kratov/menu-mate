using MenuMate.Contracts.Recipes;
using MenuMate.Modules.RecipeImports.Application.Extraction;

namespace MenuMate.Modules.RecipeImports.Application.UnitTests.Extraction;

public sealed class RecipeImportTextNormalizerTests
{
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
            "recipe.ingredients[0].amount is null because confidence=0.42",
            "JSON schema returned enum Other",
            "  "
        ]);

        Assert.Collection(
            normalized,
            warning => Assert.Equal("Проверьте количество соли: текст на изображении размыт.", warning),
            warning => Assert.Equal(
                "Проверьте распознанные данные: часть информации на изображении прочитана неуверенно.",
                warning));
    }
}
