using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.SharedKernel.UnitTests.Identifiers;

public sealed class IdentifierTests
{
    [Fact]
    public void FromShouldKeepProvidedGuid()
    {
        var value = Guid.CreateVersion7();

        Assert.Equal(value, UserId.From(value).Value);
        Assert.Equal(value, RecipeId.From(value).Value);
        Assert.Equal(value, RecipeRevisionId.From(value).Value);
    }

    [Fact]
    public void CreateShouldReturnNonEmptyIds()
    {
        Assert.NotEqual(Guid.Empty, UserId.Create().Value);
        Assert.NotEqual(Guid.Empty, RecipeId.Create().Value);
        Assert.NotEqual(Guid.Empty, RecipeRevisionId.Create().Value);
    }
}
