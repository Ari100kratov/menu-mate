namespace MenuMate.SharedKernel.UnitTests;

public sealed class ResultTests
{
    [Fact]
    public void SuccessShouldNotContainError()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.Equal(AppError.None, result.Error);
    }

    [Fact]
    public void FailureValueShouldThrowWhenValueIsAccessed()
    {
        var result = Result.Failure<string>(AppError.Validation("Code", "Message"));

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => result.Value);
        Assert.Contains("Нельзя получить значение", exception.Message, StringComparison.Ordinal);
    }
}


