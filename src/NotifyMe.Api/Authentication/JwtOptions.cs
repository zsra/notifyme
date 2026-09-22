namespace NotifyMe.Api.Authentication;

/// <summary>
/// Binds the `Jwt` configuration section used to sign/validate the self-service end-user access
/// tokens (Phase 16, ADR-0010). `SigningKey` must come from `dotnet user-secrets` locally or an
/// environment variable in CI/deployment, never committed, exactly like `Admin:ApiKey`.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "NotifyMe";

    public string Audience { get; set; } = "NotifyMe";

    public string SigningKey { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 60;
}
