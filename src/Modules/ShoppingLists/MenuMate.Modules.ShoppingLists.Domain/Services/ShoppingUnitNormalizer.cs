using MenuMate.Modules.ShoppingLists.Domain.Enums;

namespace MenuMate.Modules.ShoppingLists.Domain.Services;

/// <summary>
/// Нормализует единицы измерения для безопасного суммирования.
/// </summary>
public static class ShoppingUnitNormalizer
{
    /// <summary>
    /// Приводит кг к г и л к мл. Бытовые меры оставляет без конвертации.
    /// </summary>
    public static (decimal? Amount, ShoppingUnit Unit) Normalize(decimal? amount, ShoppingUnit unit)
    {
        if (amount is null)
        {
            return (null, unit);
        }

        return unit switch
        {
            ShoppingUnit.Kilogram => (amount.Value * 1000m, ShoppingUnit.Gram),
            ShoppingUnit.Liter => (amount.Value * 1000m, ShoppingUnit.Milliliter),
            _ => (amount.Value, unit)
        };
    }
}

