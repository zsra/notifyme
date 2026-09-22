using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NotifyMe.Application.Common.Exceptions;

namespace NotifyMe.Api.ErrorHandling;

/// <summary>
/// Maps every exception that reaches it to an RFC 7807 `ProblemDetails` response, per
/// docs/api/admin-api.md's error format: exceptions the Application layer is known to throw get
/// a specific, informative mapping; anything else falls back to a generic 500 with no exception
/// details leaked to the client (OWASP: don't expose stack traces/internals), while the full
/// exception is still logged server-side for diagnosis.
/// </summary>
public sealed class NotifyMeExceptionHandler : IExceptionHandler
{
    private readonly ILogger<NotifyMeExceptionHandler> _logger;

    public NotifyMeExceptionHandler(ILogger<NotifyMeExceptionHandler> logger)
    {
        _logger = logger;
    }

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
            ConflictException conflict => new ProblemDetails
            {
                Type = "https://example.com/errors/conflict",
                Title = "Conflict",
                Status = StatusCodes.Status409Conflict,
                Detail = conflict.Message,
            },
            AuthenticationFailedException authFailed => new ProblemDetails
            {
                Type = "https://example.com/errors/authentication-failed",
                Title = "Authentication failed",
                Status = StatusCodes.Status401Unauthorized,
                Detail = authFailed.Message,
            },
            _ => new ProblemDetails
            {
                Type = "https://example.com/errors/unexpected",
                Title = "An unexpected error occurred",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An unexpected error occurred while processing the request.",
            },
        };

        var logLevel = problemDetails.Status >= StatusCodes.Status500InternalServerError ? LogLevel.Error : LogLevel.Warning;
        _logger.Log(logLevel, exception, "Request {TraceId} failed with {StatusCode}: {ExceptionMessage}",
            httpContext.TraceIdentifier, problemDetails.Status, exception.Message);

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
