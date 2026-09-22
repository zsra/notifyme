using NotifyMe.Domain.Common;

namespace NotifyMe.Domain.Events;

/// <summary>
/// The shape the matching engine actually operates on: a <see cref="RawEvent"/> that has been
/// parsed/enriched into a title, description, and severity.
/// </summary>
public sealed class NormalizedEvent : Entity
{
    public Guid RawEventId { get; private set; }
    public EventCategory Category { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Severity Severity { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }

    private NormalizedEvent()
    {
    }

    private NormalizedEvent(
        Guid id,
        Guid rawEventId,
        EventCategory category,
        string title,
        string description,
        Severity severity,
        DateTimeOffset occurredAt)
        : base(id)
    {
        RawEventId = rawEventId;
        Category = category;
        Title = title;
        Description = description;
        Severity = severity;
        OccurredAt = occurredAt;
    }

    public static NormalizedEvent Create(
        Guid id,
        Guid rawEventId,
        EventCategory category,
        string title,
        string? description,
        Severity severity,
        DateTimeOffset occurredAt)
    {
        if (rawEventId == Guid.Empty)
        {
            throw new ArgumentException("Normalized event must reference its source raw event.", nameof(rawEventId));
        }

        if (!Enum.IsDefined(typeof(EventCategory), category))
        {
            throw new ArgumentOutOfRangeException(nameof(category), category, "Unknown event category.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Normalized event title is required.", nameof(title));
        }

        if (!Enum.IsDefined(typeof(Severity), severity))
        {
            throw new ArgumentOutOfRangeException(nameof(severity), severity, "Unknown severity.");
        }

        return new NormalizedEvent(id, rawEventId, category, title.Trim(), description?.Trim() ?? string.Empty, severity, occurredAt);
    }
}
