using MenuMate.Modules.Tags.Domain.Errors;
using MenuMate.Modules.Tags.Domain.ValueObjects;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Tags.Domain.UnitTests.ValueObjects;

public sealed class TagNameTests
{
    [Fact]
    public void CreateShouldTrimAndNormalizeName()
    {
        TagName name = TagName.Create("  Быстрый   ужин ").Value;

        Assert.Equal("Быстрый   ужин", name.Value);
        Assert.Equal("БЫСТРЫЙ УЖИН", name.NormalizedValue);
        Assert.Equal(name.Value, name.ToString());
    }

    [Fact]
    public void CreateShouldRejectEmptyName()
    {
        Result<TagName> result = TagName.Create(" ");

        Assert.True(result.IsFailure);
        Assert.Equal(TagErrors.EmptyName, result.Error);
    }

    [Fact]
    public void CreateShouldRejectNameLongerThanLimit()
    {
        Result<TagName> result = TagName.Create(new string('а', 65));

        Assert.True(result.IsFailure);
        Assert.Equal(TagErrors.NameTooLong, result.Error);
    }
}
