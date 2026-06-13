using MenuMate.Modules.ShoppingLists.Domain.Enums;
using MenuMate.Modules.ShoppingLists.Domain.Services;

namespace MenuMate.Modules.ShoppingLists.Domain.UnitTests.Services;

public sealed class ShoppingDisplayNamesTests
{
    [Fact]
    public void CategoryNamesShouldCoverEveryCategory()
    {
        foreach (ShoppingProductCategory category in Enum.GetValues<ShoppingProductCategory>())
        {
            Assert.False(string.IsNullOrWhiteSpace(ShoppingProductCategoryNames.GetDisplayName(category)));
        }
    }

    [Fact]
    public void UnitNamesShouldCoverEveryKnownUnit()
    {
        foreach (ShoppingUnit unit in Enum.GetValues<ShoppingUnit>().Where(unit => unit != ShoppingUnit.Unknown))
        {
            Assert.False(string.IsNullOrWhiteSpace(ShoppingUnitNames.GetDisplayName(unit)));
        }

        Assert.Equal(string.Empty, ShoppingUnitNames.GetDisplayName(ShoppingUnit.Unknown));
    }
}
