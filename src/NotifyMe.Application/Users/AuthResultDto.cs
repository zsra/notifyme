namespace NotifyMe.Application.Users;

/// <summary>
/// The token plus the caller's own profile, returned by both register and login so the frontend
/// never needs a second round trip to know who it just authenticated as.
/// </summary>
public sealed record AuthResultDto(string Token, DateTimeOffset ExpiresAt, UserDto User);
