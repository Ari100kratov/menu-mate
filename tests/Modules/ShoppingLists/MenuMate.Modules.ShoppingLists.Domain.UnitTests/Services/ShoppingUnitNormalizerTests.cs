using MenuMate.Modules.ShoppingLists.Domain.Enums;
using MenuMate.Modules.ShoppingLists.Domain.Services;

namespace MenuMate.Modules.ShoppingLists.Domain.UnitTests.Services;

public sealed class ShoppingUnitNormalizerTests
{
    [Theory]
    [InlineData(1, ShoppingUnit.Kilogram, 1000, ShoppingUnit.Gram)]
    [InlineData(1.5, ShoppingUnit.Liter, 1500, ShoppingUnit.Milliliter)]
    [InlineData(3, ShoppingUnit.Piece, 3, ShoppingUnit.Piece)]
    [InlineData(1, ShoppingUnit.Glass, 1, ShoppingUnit.Glass)]
    public void NormalizeShouldConvertOnlyCompatibleBaseUnits(
        decimal amount,
        ShoppingUnit unit,
        decimal expectedAmount,
        ShoppingUnit expectedUnit)
    {
        (decimal? normalizedAmount, ShoppingUnit normalizedUnit) = ShoppingUnitNormalizer.Normalize(amount, unit);

        Assert.Equal(expectedAmount, normalizedAmount);
        Assert.Equal(expectedUnit, normalizedUnit);
    }

    [Fact]
    public void NormalizeShouldKeepNullAmountAndUnit()
    {
        (decimal? amount, ShoppingUnit unit) = ShoppingUnitNormalizer.Normalize(null, ShoppingUnit.ToTaste);

        Assert.Null(amount);
        Assert.Equal(ShoppingUnit.ToTaste, unit);
    }
}
