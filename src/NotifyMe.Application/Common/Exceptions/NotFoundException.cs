namespace NotifyMe.Application.Common.Exceptions;

/// <summary>
/// Thrown when a use case looks up an entity by id and it doesn't exist. Mapped to
/// `404 Not Found` by the Api layer (Phase 08).
/// </summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message)
        : base(message)
    {
    }
}
