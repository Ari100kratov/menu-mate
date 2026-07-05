using MenuMate.Modules.Products.Infrastructure.SeedData;

namespace MenuMate.DataImporter.UnitTests;

public sealed class ProductSeedCatalogTests
{
    [Fact]
    public void CatalogShouldContainAtLeastFiveHundredRussianProducts()
    {
        Assert.True(ProductSeedCatalog.Products.Length >= 500);
        Assert.All(ProductSeedCatalog.Products, product => Assert.Matches("[А-Яа-яЁё]", product.Name));
    }
}
