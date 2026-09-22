using NotifyMe.Domain.Users;

namespace NotifyMe.Application.Abstractions;

/// <summary>
/// Issues a signed access token for a logged-in <see cref="User"/>. Implemented in the `Api`
/// project (see ADR-0010 for why token minting lives there rather than in `Infrastructure`).
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Returns the encoded token plus its absolute expiry, so the caller (the login use case)
    /// can surface both to the client.
    /// </summary>
    (string Token, DateTimeOffset ExpiresAt) GenerateToken(User user);
}
