using MenuMate.SharedKernel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MenuMate.Common.Presentation;

/// <summary>
/// Преобразует результаты сценариев приложения в HTTP-ответы.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Возвращает HTTP-ответ для результата без значения.
    /// </summary>
    public static IResult ToHttpResult(this Result result, HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(httpContext);

        return result.IsSuccess ? Results.NoContent() : ToProblem(result.Error, httpContext);
    }

    /// <summary>
    /// Возвращает HTTP-ответ для результата со значением.
    /// </summary>
    public static IResult ToHttpResult<TValue>(this Result<TValue> result, HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(httpContext);

        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error, httpContext);
    }

    private static IResult ToProblem(AppError error, HttpContext httpContext)
    {
        int statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Problem => StatusCodes.Status502BadGateway,
            _ => StatusCodes.Status500InternalServerError
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Code,
            Detail = error.Description,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        problemDetails.Extensions["code"] = error.Code;

        return Results.Problem(problemDetails);
    }
}
