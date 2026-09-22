namespace NotifyMe.Application.Common.Exceptions;

/// <summary>
/// Thrown when login credentials don't match a known user. Deliberately the same message/shape
/// regardless of whether the email or the password was wrong, so a caller can't use error
/// differences to enumerate registered emails. Mapped to `401 Unauthorized` by the Api layer.
/// </summary>
public sealed class AuthenticationFailedException : Exception
{
    public AuthenticationFailedException(string message)
        : base(message)
    {
    }
}
