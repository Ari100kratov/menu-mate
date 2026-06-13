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

    [Fact]
    public void FromTValueShouldCreateFailureForNullAndSuccessForValue()
    {
        var nullResult = Result<string>.FromTValue(null);
        var valueResult = Result<string>.FromTValue("value");

        Assert.True(nullResult.IsFailure);
        Assert.Equal(AppError.NullValue, nullResult.Error);
        Assert.Equal("value", valueResult.Value);
    }

    [Fact]
    public void ConstructorShouldRejectInvalidSuccessErrorCombination()
    {
        Assert.Throws<ArgumentException>(() => new TestResult(isSuccess: true, AppError.NullValue));
        Assert.Throws<ArgumentException>(() => new TestResult(isSuccess: false, AppError.None));
    }

    private sealed class TestResult(bool isSuccess, AppError error) : Result(isSuccess, error);
}


