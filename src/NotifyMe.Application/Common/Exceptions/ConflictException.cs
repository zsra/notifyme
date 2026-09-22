namespace NotifyMe.Application.Common.Exceptions;

/// <summary>
/// Thrown when a use case would create a duplicate of something that must be unique (e.g. an
/// email address already registered). Mapped to `409 Conflict` by the Api layer.
/// </summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message)
    {
    }
}
