using MenuMate.Modules.ShoppingLists.Domain.Enums;
using MenuMate.Modules.ShoppingLists.Domain.Models;
using MenuMate.Modules.ShoppingLists.Domain.Services;
using MenuMate.Modules.ShoppingLists.Domain.ValueObjects;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.ShoppingLists.Domain.UnitTests.Services;

public sealed class ShoppingListGeneratorTests
{
    [Fact]
    public void GenerateShouldScaleNormalizeAndMergeCompatibleItemsByProduct()
    {
        var productId = Guid.CreateVersion7();
        ShoppingRecipe firstRecipe = CreateRecipe(
            baseServings: 2,
            targetServings: 4,
            [CreateIngredient(productId, 0.2m, ShoppingUnit.Kilogram)]);
        ShoppingRecipe secondRecipe = CreateRecipe(
            baseServings: 1,
            targetServings: 1,
            [CreateIngredient(productId, 100m, ShoppingUnit.Gram)]);

        ShoppingList list = ShoppingListGenerator.Generate([firstRecipe, secondRecipe]);

        ShoppingListItem item = Assert.Single(Assert.Single(list.Categories).Items);
        Assert.Equal(500m, item.Amount);
        Assert.Equal(ShoppingUnit.Gram, item.Unit);
    }

    [Fact]
    public void GenerateShouldNotMergeSameNameWithDifferentProductIds()
    {
        ShoppingRecipe recipe = CreateRecipe(
            1,
            1,
            [
                CreateIngredient(Guid.CreateVersion7(), 100m, ShoppingUnit.Gram),
                CreateIngredient(Guid.CreateVersion7(), 100m, ShoppingUnit.Gram)
            ]);

        ShoppingList list = ShoppingListGenerator.Generate([recipe]);

        Assert.Equal(2, Assert.Single(list.Categories).Items.Count);
    }

    [Fact]
    public void GenerateShouldKeepToTasteLinesSeparateAndMarkOptionalComment()
    {
        ShoppingRecipe recipe = CreateRecipe(
            2,
            4,
            [
                CreateIngredient(Guid.CreateVersion7(), null, ShoppingUnit.ToTaste, "для подачи", true),
                CreateIngredient(Guid.CreateVersion7(), null, ShoppingUnit.ToTaste, "по вкусу", false)
            ]);

        ShoppingList list = ShoppingListGenerator.Generate([recipe]);

        ShoppingListItem[] items = [.. Assert.Single(list.Categories).Items];
        Assert.Equal(2, items.Length);
        Assert.Contains(items, item => item.Comment == "для подачи; необязательно");
        Assert.All(items, item => Assert.False(item.CanMerge));
    }

    [Fact]
    public void GenerateShouldIncludeAndNormalizeManualLines()
    {
        var productId = Guid.CreateVersion7();
        var manualLine = new ManualShoppingListLine(
            productId,
            "Молоко",
            "МОЛОКО",
            1m,
            ShoppingUnit.Liter,
            ShoppingProductCategory.Dairy,
            null);

        ShoppingList list = ShoppingListGenerator.Generate([], [manualLine]);

        ShoppingListItem item = Assert.Single(Assert.Single(list.Categories).Items);
        Assert.Equal(productId, item.ProductId);
        Assert.Equal(1000m, item.Amount);
        Assert.Equal(ShoppingUnit.Milliliter, item.Unit);
    }

    private static ShoppingRecipe CreateRecipe(
        int baseServings,
        int targetServings,
        IReadOnlyCollection<ShoppingIngredientLine> ingredients) =>
        new(RecipeId.Create(), baseServings, targetServings, ingredients);

    private static ShoppingIngredientLine CreateIngredient(
        Guid productId,
        decimal? amount,
        ShoppingUnit unit,
        string? comment = null,
        bool isOptional = false) =>
        new(
            productId,
            "Мука",
            "МУКА",
            amount,
            unit,
            ShoppingProductCategory.GrainsAndPasta,
            comment,
            isOptional);
}
