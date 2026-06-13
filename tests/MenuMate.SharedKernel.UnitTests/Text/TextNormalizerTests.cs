namespace MenuMate.SharedKernel.UnitTests.Text;

public sealed class TextNormalizerTests
{
    [Fact]
    public void NormalizeSearchTextShouldTrimCollapseWhitespaceAndUppercase()
    {
        string result = TextNormalizer.NormalizeSearchText("  Рис \t басмати \r\n ");

        Assert.Equal("РИС БАСМАТИ", result);
    }

    [Fact]
    public void NormalizeSearchTextShouldRejectNull()
    {
        Assert.Throws<ArgumentNullException>(() => TextNormalizer.NormalizeSearchText(null!));
    }
}
