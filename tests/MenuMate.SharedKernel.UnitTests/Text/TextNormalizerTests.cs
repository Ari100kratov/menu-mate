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

    [Fact]
    public void ProductNameNormalizerShouldLowercaseReplaceYoAndCollapseWhitespace()
    {
        string result = ProductNameNormalizer.Normalize("  Ёлки\tПАЛКИ \r\n");

        Assert.Equal("елки палки", result);
        Assert.Equal("ЕЛКИ ПАЛКИ", ProductNameNormalizer.NormalizeForComparison(result));
    }
}
