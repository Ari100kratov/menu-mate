using MenuMate.Modules.Auth.Domain.ValueObjects;

namespace MenuMate.Modules.Auth.Domain.UnitTests.ValueObjects;

public sealed class EmailNormalizerTests
{
    [Fact]
    public void NormalizeShouldTrimAndLowercaseEmail()
    {
        string normalized = EmailNormalizer.Normalize("  User@Example.COM ");

        Assert.Equal("user@example.com", normalized);
    }

    [Fact]
    public void NormalizeShouldRejectNull()
    {
        Assert.Throws<ArgumentNullException>(() => EmailNormalizer.Normalize(null!));
    }
}
