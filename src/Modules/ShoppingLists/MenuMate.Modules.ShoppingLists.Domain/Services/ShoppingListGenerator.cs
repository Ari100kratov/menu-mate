using MenuMate.Modules.ShoppingLists.Domain.Enums;
using MenuMate.Modules.ShoppingLists.Domain.Models;
using MenuMate.Modules.ShoppingLists.Domain.ValueObjects;

namespace MenuMate.Modules.ShoppingLists.Domain.Services;

/// <summary>
/// Генерирует список покупок из рецептов меню и ручных позиций.
/// </summary>
public static class ShoppingListGenerator
{
    /// <summary>
    /// Формирует список покупок.
    /// </summary>
    public static ShoppingList Generate(
        IEnumerable<ShoppingRecipe> recipes,
        IEnumerable<ManualShoppingListLine>? manualLines = null)
    {
        ArgumentNullException.ThrowIfNull(recipes);

        var items = new List<ShoppingListItem>();

        foreach (ShoppingRecipe recipe in recipes)
        {
            decimal factor = recipe.BaseServings <= 0 ? 1m : (decimal)recipe.TargetServings / recipe.BaseServings;
            items.AddRange(recipe.Ingredients.Select(ingredient => CreateFromIngredient(ingredient, factor)));
        }

        if (manualLines is not null)
        {
            items.AddRange(manualLines.Select(CreateFromManualLine));
        }

        ShoppingListItem[] mergedItems =
        [
            .. items
                .Where(item => item.CanMerge)
                .GroupBy(item => new MergeKey(item.ProductId, item.Unit, item.QuantityKind))
                .Select(Merge)
                .Concat(items.Where(item => !item.CanMerge))
        ];

        return ShoppingList.FromItems(mergedItems);
    }

    private static ShoppingListItem CreateFromIngredient(ShoppingIngredientLine ingredient, decimal factor)
    {
        decimal? scaledAmount = ingredient.Amount.HasValue
            ? Math.Round(ingredient.Amount.Value * factor, 2)
            : null;

        (decimal? amount, ShoppingUnit unit) = ShoppingUnitNormalizer.Normalize(scaledAmount, ingredient.Unit);

        string? comment = ingredient.IsOptional
            ? JoinComments(ingredient.Comment, "необязательно")
            : ingredient.Comment;

        return new ShoppingListItem(
            ingredient.ProductId,
            ingredient.Name,
            ingredient.NormalizedName,
            amount,
            unit,
            ingredient.QuantityKind,
            ingredient.Category,
            comment);
    }

    private static ShoppingListItem CreateFromManualLine(ManualShoppingListLine line)
    {
        (decimal? amount, ShoppingUnit unit) = ShoppingUnitNormalizer.Normalize(line.Amount, line.Unit);

        return new ShoppingListItem(
            line.ProductId,
            line.Name,
            line.NormalizedName,
            amount,
            unit,
            line.QuantityKind,
            line.Category,
            line.Comment);
    }

    private static ShoppingListItem Merge(IGrouping<MergeKey, ShoppingListItem> group)
    {
        ShoppingListItem first = group.First();
        decimal amount = group.Sum(item => item.Amount.GetValueOrDefault());
        string? comment = JoinComments(group.Select(item => item.Comment).Where(comment => !string.IsNullOrWhiteSpace(comment)));

        return new ShoppingListItem(
            first.ProductId,
            first.Name,
            first.NormalizedName,
            amount,
            first.Unit,
            first.QuantityKind,
            first.Category,
            comment);
    }

    private static string? JoinComments(params string?[] comments) => JoinComments(comments.AsEnumerable());

    private static string? JoinComments(IEnumerable<string?> comments)
    {
        string[] nonEmptyComments =
        [
            .. comments
                .Where(comment => !string.IsNullOrWhiteSpace(comment))
                .Distinct(StringComparer.CurrentCultureIgnoreCase)
                .Select(comment => comment!.Trim())
        ];

        return nonEmptyComments.Length == 0 ? null : string.Join("; ", nonEmptyComments);
    }

    private readonly record struct MergeKey(
        Guid ProductId,
        ShoppingUnit Unit,
        ShoppingQuantityKind QuantityKind);
}
