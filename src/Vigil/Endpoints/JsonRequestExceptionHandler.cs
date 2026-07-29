using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Vigil.Endpoints;

internal sealed class JsonRequestExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // ASP.NET Core wraps JSON body-binding failures in BadHttpRequestException, but only
        // for some exception types (e.g. JsonException) - others (e.g. NotSupportedException,
        // thrown for a missing polymorphic type discriminator) propagate unwrapped. Check both.
        var jsonException = exception switch
        {
            JsonException or NotSupportedException => exception,
            BadHttpRequestException { InnerException: JsonException or NotSupportedException } badRequest =>
                badRequest.InnerException,
            _ => null
        };

        if (jsonException is null)
            return false;

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request",
            Detail = jsonException.Message
        }, cancellationToken);

        return true;
    }
}
