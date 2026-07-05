using MenuMate.DataImporter.Wikibooks;

namespace MenuMate.DataImporter.UnitTests;

public sealed class ImageLicensePolicyTests
{
    [Theory]
    [InlineData("CC BY 4.0")]
    [InlineData("CC BY-SA 3.0")]
    [InlineData("CC0 1.0")]
    [InlineData("Public domain")]
    public void AllowedLicenseShouldBeAccepted(string license)
    {
        Assert.True(ImageLicensePolicy.IsAllowed(license));
    }

    [Theory]
    [InlineData("")]
    [InlineData("CC BY-NC 4.0")]
    [InlineData("All rights reserved")]
    public void UnsupportedLicenseShouldBeRejected(string license)
    {
        Assert.False(ImageLicensePolicy.IsAllowed(license));
    }
}
