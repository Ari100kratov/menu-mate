namespace MenuMate.SharedKernel.UnitTests.Errors;

public sealed class DomainExceptionTests
{
    [Fact]
    public void ErrorConstructorShouldExposeDomainError()
    {
        var error = AppError.Validation("Domain.Invalid", "Нарушен инвариант.");

        var exception = new DomainException(error);

        Assert.Equal(error, exception.Error);
        Assert.Equal(error.Description, exception.Message);
    }

    [Fact]
    public void MessageAndInnerExceptionConstructorShouldPreserveInnerException()
    {
        var inner = new InvalidOperationException("inner");

        var exception = new DomainException("Нарушен инвариант.", inner);

        Assert.Same(inner, exception.InnerException);
        Assert.Equal("Domain.Exception", exception.Error.Code);
    }
}
