using MenuMate.Modules.ShoppingLists.Domain.Enums;
using MenuMate.Modules.ShoppingLists.Domain.Models;
using MenuMate.Modules.ShoppingLists.Domain.Services;
using MenuMate.Modules.ShoppingLists.Domain.ValueObjects;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.ShoppingLists.Domain.UnitTests;

public sealed class ShoppingListGeneratorTests
{
    [Fact]
    public void GenerateShouldScaleNormalizeAndMergeCompatibleItems()
    {
        ShoppingRecipe firstRecipe = new(
            RecipeId.Create(),
            BaseServings: 2,
            TargetServings: 4,
            Ingredients:
            [
                new ShoppingIngredientLine(
                    "Мука",
                    "мука",
                    0.2m,
                    ShoppingUnit.Kilogram,
                    ShoppingQuantityKind.Exact,
                    ShoppingProductCategory.GrainsAndPasta,
                    null,
                    false)
            ]);
        ShoppingRecipe secondRecipe = new(
            RecipeId.Create(),
            BaseServings: 1,
            TargetServings: 1,
            Ingredients:
            [
                new ShoppingIngredientLine(
                    "мука",
                    "мука",
                    100m,
                    ShoppingUnit.Gram,
                    ShoppingQuantityKind.Exact,
                    ShoppingProductCategory.GrainsAndPasta,
                    null,
                    false)
            ]);

        ShoppingList list = ShoppingListGenerator.Generate([firstRecipe, secondRecipe]);

        ShoppingListItem item = list.Categories.Single().Items.Single();
        Assert.Equal("Мука", item.Name);
        Assert.Equal(500m, item.Amount);
        Assert.Equal(ShoppingUnit.Gram, item.Unit);
    }

    [Fact]
    public void GenerateShouldKeepUncertainQuantitiesSeparate()
    {
        ShoppingRecipe recipe = new(
            RecipeId.Create(),
            BaseServings: 2,
            TargetServings: 4,
            Ingredients:
            [
                new ShoppingIngredientLine(
                    "Соль",
                    "соль",
                    null,
                    ShoppingUnit.ToTaste,
                    ShoppingQuantityKind.ToTaste,
                    ShoppingProductCategory.Spices,
                    "по вкусу",
                    false),
                new ShoppingIngredientLine(
                    "соль",
                    "соль",
                    null,
                    ShoppingUnit.ToTaste,
                    ShoppingQuantityKind.ToTaste,
                    ShoppingProductCategory.Spices,
                    "для подачи",
                    false)
            ]);

        ShoppingList list = ShoppingListGenerator.Generate([recipe]);

        ShoppingListItem[] items = [.. list.Categories.Single().Items];
        Assert.Equal(2, items.Length);
        Assert.All(items, item => Assert.False(item.CanMerge));
    }

    [Fact]
    public void FormatShouldCreateCopyableTextGroupedByCategory()
    {
        var list = ShoppingList.FromItems(
        [
            new ShoppingListItem(
                "Молоко",
                "молоко",
                750m,
                ShoppingUnit.Milliliter,
                ShoppingQuantityKind.Exact,
                ShoppingProductCategory.Dairy,
                null)
        ]);

        string text = ShoppingListTextFormatter.Format(list);

        Assert.Contains("Молочные продукты", text, StringComparison.Ordinal);
        Assert.Contains("- [ ] Молоко 750 мл", text, StringComparison.Ordinal);
    }
}
