using NotifyMe.Domain.Common;

namespace NotifyMe.Domain.Events;

/// <summary>
/// An event exactly as produced by an <see cref="Abstractions.IEventSource"/>, before it has
/// been normalized into the shape the matching engine operates on. See
/// docs/architecture/event-ingestion.md for the ingestion pipeline this participates in.
/// </summary>
public sealed class RawEvent : Entity
{
    public string Source { get; private set; } = string.Empty;
    public EventCategory Category { get; private set; }
    public string Payload { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; private set; }

    private RawEvent()
    {
    }

    private RawEvent(Guid id, string source, EventCategory category, string payload, DateTimeOffset occurredAt)
        : base(id)
    {
        Source = source;
        Category = category;
        Payload = payload;
        OccurredAt = occurredAt;
    }

    public static RawEvent Create(Guid id, string source, EventCategory category, string payload, DateTimeOffset occurredAt)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentException("Raw event source is required.", nameof(source));
        }

        if (!Enum.IsDefined(typeof(EventCategory), category))
        {
            throw new ArgumentOutOfRangeException(nameof(category), category, "Unknown event category.");
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new ArgumentException("Raw event payload is required.", nameof(payload));
        }

        return new RawEvent(id, source.Trim(), category, payload, occurredAt);
    }
}
