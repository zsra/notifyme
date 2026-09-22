using System.Text;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NotifyMe.Api.ErrorHandling;
using NotifyMe.Application.Common.Exceptions;
using Xunit;

namespace NotifyMe.IntegrationTests.Api;

/// <summary>
/// Exercises <see cref="NotifyMeExceptionHandler"/> directly (rather than through a full HTTP
/// round trip) so every mapped exception type - including the catch-all "anything else" branch
/// added in Phase 10 - is covered. The catch-all 500 path in particular was previously untested
/// (0% line coverage per the Phase 11 coverage review), since no endpoint in the suite happens
/// to throw an unrecognized exception type.
/// </summary>
public class NotifyMeExceptionHandlerTests
{
    private readonly NotifyMeExceptionHandler _handler = new(NullLogger<NotifyMeExceptionHandler>.Instance);

    [Fact]
    public async Task TryHandleAsync_UnrecognizedException_Returns500WithGenericDetail()
    {
        var (httpContext, bodyStream) = CreateHttpContext();

        var handled = await _handler.TryHandleAsync(httpContext, new InvalidOperationException("Something specific and internal broke."), CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);

        var problemDetails = await ReadProblemDetailsAsync(bodyStream);
        Assert.Equal("An unexpected error occurred while processing the request.", problemDetails.Detail);
        Assert.DoesNotContain("Something specific and internal broke.", await ReadRawBodyAsync(bodyStream));
    }

    [Fact]
    public async Task TryHandleAsync_NotFoundException_Returns404WithMessage()
    {
        var (httpContext, bodyStream) = CreateHttpContext();

        var handled = await _handler.TryHandleAsync(httpContext, new NotFoundException("Alert rule 'x' was not found."), CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status404NotFound, httpContext.Response.StatusCode);

        var problemDetails = await ReadProblemDetailsAsync(bodyStream);
        Assert.Equal("Alert rule 'x' was not found.", problemDetails.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_ValidationException_Returns400WithErrorMessages()
    {
        var (httpContext, bodyStream) = CreateHttpContext();
        var validationException = new ValidationException(new[]
        {
            new FluentValidation.Results.ValidationFailure("Name", "'Name' must not be empty."),
        });

        var handled = await _handler.TryHandleAsync(httpContext, validationException, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);

        var problemDetails = await ReadProblemDetailsAsync(bodyStream);
        Assert.Contains("'Name' must not be empty.", problemDetails.Detail);
    }

    private static (DefaultHttpContext HttpContext, MemoryStream BodyStream) CreateHttpContext()
    {
        var httpContext = new DefaultHttpContext();
        var bodyStream = new MemoryStream();
        httpContext.Response.Body = bodyStream;
        return (httpContext, bodyStream);
    }

    private static async Task<ProblemDetails> ReadProblemDetailsAsync(MemoryStream bodyStream)
    {
        var json = await ReadRawBodyAsync(bodyStream);
        return JsonSerializer.Deserialize<ProblemDetails>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    private static Task<string> ReadRawBodyAsync(MemoryStream bodyStream)
    {
        bodyStream.Seek(0, SeekOrigin.Begin);
        return Task.FromResult(Encoding.UTF8.GetString(bodyStream.ToArray()));
    }
}
