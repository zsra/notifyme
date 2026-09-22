using NotifyMe.Domain.Common;
using NotifyMe.Domain.Events;

namespace NotifyMe.Domain.Alerts;

/// <summary>
/// A user-configured rule: notify subscribers when an event in <see cref="Category"/> satisfies
/// <see cref="Criteria"/>. See docs/architecture/data-model.md for how this relates to
/// <see cref="Subscriptions.Subscription"/> and <see cref="Channels.ChannelConfig"/>.
/// </summary>
public sealed class AlertRule : Entity
{
    public string Name { get; private set; } = string.Empty;
    public EventCategory Category { get; private set; }
    public MatchCriteria Criteria { get; private set; } = null!;
    public bool IsEnabled { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// <c>null</c> means admin/global-owned (every rule created through the Admin API, unchanged
    /// since Phase 08). A non-null value is the <see cref="Users.User"/> who owns this rule via
    /// the Phase 16 self-service `/api/me` surface; see ADR-0010.
    /// </summary>
    public Guid? OwnerUserId { get; private set; }

    private AlertRule()
    {
    }

    private AlertRule(Guid id, string name, EventCategory category, MatchCriteria criteria, bool isEnabled, DateTimeOffset createdAt, Guid? ownerUserId)
        : base(id)
    {
        Name = name;
        Category = category;
        Criteria = criteria;
        IsEnabled = isEnabled;
        CreatedAt = createdAt;
        OwnerUserId = ownerUserId;
    }

    public static AlertRule Create(
        Guid id,
        string name,
        EventCategory category,
        MatchCriteria criteria,
        DateTimeOffset createdAt,
        bool isEnabled = true,
        Guid? ownerUserId = null)
    {
        EnsureValid(name, category);
        ArgumentNullException.ThrowIfNull(criteria);

        return new AlertRule(id, name.Trim(), category, criteria, isEnabled, createdAt, ownerUserId);
    }

    public void Enable() => IsEnabled = true;

    public void Disable() => IsEnabled = false;

    /// <summary>
    /// Replaces the editable details of this rule (name, category, match criteria). Added in
    /// Phase 04 because the Application layer's `UpdateAlertRule` use case needs a way to edit
    /// an existing rule without discarding its identity/`CreatedAt`/`IsEnabled` state.
    /// </summary>
    public void UpdateDetails(string name, EventCategory category, MatchCriteria criteria)
    {
        EnsureValid(name, category);
        ArgumentNullException.ThrowIfNull(criteria);

        Name = name.Trim();
        Category = category;
        Criteria = criteria;
    }

    private static void EnsureValid(string name, EventCategory category)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Alert rule name is required.", nameof(name));
        }

        if (!Enum.IsDefined(typeof(EventCategory), category))
        {
            throw new ArgumentOutOfRangeException(nameof(category), category, "Unknown event category.");
        }
    }

    /// <summary>
    /// Whether this rule (if enabled) should fire for the given event. Disabled rules never
    /// match, regardless of criteria, so callers don't need to check <see cref="IsEnabled"/>
    /// separately.
    /// </summary>
    public bool Matches(NormalizedEvent normalizedEvent)
    {
        ArgumentNullException.ThrowIfNull(normalizedEvent);

        return IsEnabled
            && Category == normalizedEvent.Category
            && Criteria.IsSatisfiedBy(normalizedEvent.Title, normalizedEvent.Description, normalizedEvent.Severity);
    }
}
