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
