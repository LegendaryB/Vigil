using Ardalis.Result;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Vigil.Endpoints;

public static class ResultExtensions
{
    public static IResult ToProblemDetails<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return Results.Ok(result.Value);

        return result.Status switch
        {
            ResultStatus.Invalid => Results.ValidationProblem(
                errors: result.ValidationErrors
                    .GroupBy(e => e.Identifier ?? string.Empty)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    ),
                title: "One or more validation errors occurred.",
                statusCode: StatusCodes.Status400BadRequest
            ),

            ResultStatus.NotFound => Results.Problem(
                title: "Resource Not Found",
                detail: string.Join("; ", result.Errors),
                statusCode: StatusCodes.Status404NotFound
            ),

            ResultStatus.Unauthorized => Results.Problem(
                title: "Unauthorized",
                detail: string.Join("; ", result.Errors),
                statusCode: StatusCodes.Status401Unauthorized
            ),

            ResultStatus.Forbidden => Results.Problem(
                title: "Forbidden",
                detail: string.Join("; ", result.Errors),
                statusCode: StatusCodes.Status403Forbidden
            ),

            _ => Results.Problem(
                title: "An error occurred processing your request.",
                detail: string.Join("; ", result.Errors),
                statusCode: StatusCodes.Status400BadRequest
            )
        };
    }
}