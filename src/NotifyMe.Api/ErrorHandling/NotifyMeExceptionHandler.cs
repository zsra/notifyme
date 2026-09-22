using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NotifyMe.Application.Common.Exceptions;

namespace NotifyMe.Api.ErrorHandling;

/// <summary>
/// Maps exceptions the Application layer is known to throw to RFC 7807 `ProblemDetails`
/// responses, per docs/api/admin-api.md's error format. Anything not recognized here is left
/// unhandled (returns <c>false</c>) so the default developer/production exception page still
/// applies and the failure isn't silently swallowed.
/// </summary>
public sealed class NotifyMeExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            NotFoundException notFound => new ProblemDetails
            {
                Type = "https://example.com/errors/not-found",
                Title = "Resource not found",
                Status = StatusCodes.Status404NotFound,
                Detail = notFound.Message,
            },
            ValidationException validation => new ProblemDetails
            {
                Type = "https://example.com/errors/validation-failed",
                Title = "Validation failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = string.Join(" ", validation.Errors.Select(error => error.ErrorMessage)),
            },
            ArgumentException argument => new ProblemDetails
            {
                Type = "https://example.com/errors/validation-failed",
                Title = "Validation failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = argument.Message,
            },
            _ => null,
        };

        if (problemDetails is null)
        {
            return false;
        }

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
