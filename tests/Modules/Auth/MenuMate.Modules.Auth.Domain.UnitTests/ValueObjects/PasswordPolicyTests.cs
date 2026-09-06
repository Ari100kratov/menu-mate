using MenuMate.Modules.Auth.Domain.ValueObjects;

namespace MenuMate.Modules.Auth.Domain.UnitTests.ValueObjects;

public sealed class PasswordPolicyTests
{
    [Theory]
    [InlineData(7, false)]
    [InlineData(8, true)]
    [InlineData(128, true)]
    [InlineData(129, false)]
    public void PasswordLengthShouldRespectBoundaries(int length, bool expected)
    {
        Assert.Equal(expected, PasswordPolicy.IsValid(new string('a', length)));
    }
}
