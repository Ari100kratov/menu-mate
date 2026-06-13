namespace MenuMate.SharedKernel.UnitTests.Errors;

public sealed class AppErrorTests
{
    [Theory]
    [InlineData(ErrorType.Failure)]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Unauthorized)]
    [InlineData(ErrorType.Forbidden)]
    [InlineData(ErrorType.Problem)]
    public void FactoryShouldCreateExpectedErrorType(ErrorType type)
    {
        AppError error = type switch
        {
            ErrorType.Failure => AppError.Failure("Code", "Description"),
            ErrorType.Validation => AppError.Validation("Code", "Description"),
            ErrorType.NotFound => AppError.NotFound("Code", "Description"),
            ErrorType.Conflict => AppError.Conflict("Code", "Description"),
            ErrorType.Unauthorized => AppError.Unauthorized("Code", "Description"),
            ErrorType.Forbidden => AppError.Forbidden("Code", "Description"),
            _ => AppError.Problem("Code", "Description")
        };

        Assert.Equal("Code", error.Code);
        Assert.Equal("Description", error.Description);
        Assert.Equal(type, error.Type);
    }
}
