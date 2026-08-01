using System.Globalization;
using System.Text;
using MenuMate.Modules.ShoppingLists.Domain.Enums;
using MenuMate.Modules.ShoppingLists.Domain.Models;

namespace MenuMate.Modules.ShoppingLists.Domain.Services;

/// <summary>
/// Формирует текстовую версию списка покупок для копирования и шаринга.
/// </summary>
public static class ShoppingListTextFormatter
{
    /// <summary>
    /// Формирует текст списка покупок.
    /// </summary>
    public static string Format(
        ShoppingList shoppingList,
        ShoppingListTextScope scope = ShoppingListTextScope.All)
    {
        ArgumentNullException.ThrowIfNull(shoppingList);

        var builder = new StringBuilder();

        foreach (ShoppingListCategory category in shoppingList.Categories)
        {
            ShoppingListItem[] items =
            [
                .. category.Items.Where(item => scope == ShoppingListTextScope.All || !item.IsPurchased)
            ];
            if (items.Length == 0)
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.AppendLine(category.Name);

            foreach (ShoppingListItem item in items)
            {
                builder.Append(item.IsPurchased ? "- ✓ " : "- ");
                builder.Append(item.Name);

                string amountText = FormatAmount(item);
                if (amountText.Length > 0)
                {
                    builder.Append(' ');
                    builder.Append(amountText);
                }

                if (!string.IsNullOrWhiteSpace(item.Comment))
                {
                    builder.Append(" (");
                    builder.Append(item.Comment);
                    builder.Append(')');
                }

                builder.AppendLine();
            }
        }

        return builder.ToString().TrimEnd();
    }

    /// <summary>
    /// Формирует текст количества позиции.
    /// </summary>
    public static string FormatAmount(ShoppingListItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item.Unit == ShoppingUnit.ToTaste)
        {
            return ShoppingUnitNames.GetDisplayName(ShoppingUnit.ToTaste);
        }

        if (item.Amount is null)
        {
            return ShoppingUnitNames.GetDisplayName(item.Unit);
        }

        string unit = ShoppingUnitNames.GetDisplayName(item.Unit);
        string amount = item.Amount.Value.ToString("0.##", CultureInfo.InvariantCulture);
        return unit.Length == 0 ? amount : $"{amount} {unit}";
    }
}

/// <summary>
/// Набор позиций, включаемых в текстовую версию списка покупок.
/// </summary>
public enum ShoppingListTextScope
{
    /// <summary>
    /// Все позиции с текущими отметками.
    /// </summary>
    All,

    /// <summary>
    /// Только позиции, которые еще не куплены.
    /// </summary>
    Remaining
}
