namespace NotifyMe.Api.Authentication;

/// <summary>
/// Dev-grade Admin API auth per ADR-0006: every <c>/api/admin/*</c> request must carry an
/// <c>X-Api-Key</c> header matching the key configured under <c>Admin:ApiKey</c>
/// (`dotnet user-secrets` locally, an environment variable in CI/deployment - never
/// committed). Requests are rejected (`401`) if the header is missing/wrong, and also if the
/// server itself has no key configured - a missing configuration value fails closed rather than
/// leaving the admin surface open.
/// </summary>
public sealed class ApiKeyEndpointFilter : IEndpointFilter
{
    public const string HeaderName = "X-Api-Key";

    private readonly IConfiguration _configuration;

    public ApiKeyEndpointFilter(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var expectedApiKey = _configuration["Admin:ApiKey"];

        if (string.IsNullOrEmpty(expectedApiKey))
        {
            return Results.Problem(
                title: "Admin API key is not configured",
                detail: "The server has no Admin:ApiKey configured, so all admin requests are rejected.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var providedApiKey = context.HttpContext.Request.Headers[HeaderName].ToString();

        if (string.IsNullOrEmpty(providedApiKey) || !SecureEquals(providedApiKey, expectedApiKey))
        {
            return Results.Problem(
                title: "Missing or invalid API key",
                detail: $"A valid '{HeaderName}' header is required for this endpoint.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        return await next(context);
    }

    private static bool SecureEquals(string a, string b)
    {
        var bytesA = System.Text.Encoding.UTF8.GetBytes(a);
        var bytesB = System.Text.Encoding.UTF8.GetBytes(b);
        return bytesA.Length == bytesB.Length && System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(bytesA, bytesB);
    }
}
