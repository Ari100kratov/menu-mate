using MenuMate.Contracts.Recipes;
using MenuMate.DataImporter.Recipes;
using MenuMate.DataImporter.Wikibooks;

namespace MenuMate.DataImporter.UnitTests;

public sealed class WikibooksRecipeParserTests
{
    [Fact]
    public void ParseShouldCreateRecipeAndRemoveStepNumbers()
    {
        var page = new WikibooksPage(
            1,
            2,
            "Рисовая каша",
            """
            == Ингредиенты ==
            * 1 стакан риса
            * 500 мл молока

            == Приготовление ==
            # 1. Промыть рис.
            # 2. Сварить рис в молоке.
            """,
            new Uri("https://example.com/recipe"),
            null);

        CreateRecipeRequest? recipe = WikibooksRecipeParser.Parse(page);

        Assert.NotNull(recipe);
        Assert.Equal("Glass", recipe.Ingredients.First().Unit);
        Assert.Equal("Промыть рис.", recipe.Steps.First().Text);
    }

    [Fact]
    public void ParseShouldReadNameFirstIngredientsFromCompositionSection()
    {
        var page = new WikibooksPage(
            32074,
            1,
            "Рецепт:Бабаоцай",
            """
            == Бабаоцай 1 ==
            === Состав ===
            * свинина: 100 г (нарезать тонкими ломтиками)
            * кальмар: 1 шт
            * креветки: 5 шт
            * соевый соус
            === Приготовление ===
            # Нарезать свинину.
            # Обжарить все ингредиенты.

            == Бабаоцай 2 ==
            === Состав ===
            * курица: 2 кг
            === Приготовление ===
            # Этот вариант не должен попасть в результат.
            """,
            new Uri("https://ru.wikibooks.org/wiki/Рецепт:Бабаоцай"),
            null);

        CreateRecipeRequest? recipe = WikibooksRecipeParser.Parse(page);

        Assert.NotNull(recipe);
        Assert.Equal("Бабаоцай", recipe.Title);
        Assert.Equal(4, recipe.Ingredients.Count);
        Assert.Equal("Gram", recipe.Ingredients.First().Unit);
        Assert.Equal("ToTaste", recipe.Ingredients.Last().Unit);
        Assert.Equal(2, recipe.Steps.Count);
    }

    [Fact]
    public void ParseShouldReadFirstFreeFormVariant()
    {
        var page = new WikibooksPage(
            6652,
            1,
            "Рецепт:Аджика",
            """
            {{Рецепт
            | Категория = Соусы
            }}

            = Грузинская аджика =
            == АДЖИКА КРАСНАЯ ГРУЗИНСКАЯ ==
            1кг стручкового сухого острого красного перца, 50–70 г семян кориандра, 100 г хмели-сунели, немного корицы, 200 г грецких орехов, 300-400 г крупной соли, примерно 300 г чеснока.

            Замочить на 1 час острый красный перец.

            Добавить остальные ингредиенты.

            Пропустить через мясорубку.

            == АДЖИКА ГРУЗИНСКАЯ № 2 ==
            5 кг помидоров, 1 кг чеснока, 500 г перца.

            Этот вариант не должен попасть в результат.
            """,
            new Uri("https://ru.wikibooks.org/wiki/Рецепт:Аджика"),
            null);

        CreateRecipeRequest? recipe = WikibooksRecipeParser.Parse(page);

        Assert.NotNull(recipe);
        Assert.Equal("АДЖИКА КРАСНАЯ ГРУЗИНСКАЯ", recipe.Title);
        Assert.Equal("Sauce", recipe.Category);
        Assert.Equal(6, recipe.Ingredients.Count);
        Assert.Equal(3, recipe.Steps.Count);
        Assert.DoesNotContain(recipe.Ingredients, ingredient => ingredient.ProductName.Contains("помидор", StringComparison.OrdinalIgnoreCase));
    }
}
