using System.Security.Claims;

namespace NotifyMe.Api.Authentication;

/// <summary>
/// Extracts the caller's own user id from the JWT claims set by <see cref="JwtTokenGenerator"/>,
/// for the `/api/me/*` endpoints to scope their use case calls to (see ADR-0010).
/// </summary>
public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(value) || !Guid.TryParse(value, out var userId))
        {
            throw new InvalidOperationException("The authenticated principal has no valid user id claim.");
        }

        return userId;
    }
}
