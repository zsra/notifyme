using NotifyMe.Domain.Common;

namespace NotifyMe.Domain.Notifications;

/// <summary>
/// One row per dispatch attempt: which rule matched, which normalized event triggered it, which
/// channel it was sent through, and how that attempt turned out. Queryable via the Admin API's
/// notification history endpoint (see docs/api/admin-api.md).
/// </summary>
public sealed class Notification : Entity
{
    public Guid AlertRuleId { get; private set; }
    public Guid ChannelConfigId { get; private set; }
    public Guid NormalizedEventId { get; private set; }
    public NotificationStatus Status { get; private set; }
    public DateTimeOffset? SentAt { get; private set; }
    public string? Error { get; private set; }

    private Notification()
    {
    }

    private Notification(Guid id, Guid alertRuleId, Guid channelConfigId, Guid normalizedEventId)
        : base(id)
    {
        AlertRuleId = alertRuleId;
        ChannelConfigId = channelConfigId;
        NormalizedEventId = normalizedEventId;
        Status = NotificationStatus.Pending;
    }

    public static Notification CreatePending(Guid id, Guid alertRuleId, Guid channelConfigId, Guid normalizedEventId)
    {
        if (alertRuleId == Guid.Empty)
        {
            throw new ArgumentException("Notification must reference an alert rule.", nameof(alertRuleId));
        }

        if (channelConfigId == Guid.Empty)
        {
            throw new ArgumentException("Notification must reference a channel.", nameof(channelConfigId));
        }

        if (normalizedEventId == Guid.Empty)
        {
            throw new ArgumentException("Notification must reference the event that triggered it.", nameof(normalizedEventId));
        }

        return new Notification(id, alertRuleId, channelConfigId, normalizedEventId);
    }

    public void MarkSent(DateTimeOffset sentAt)
    {
        if (Status != NotificationStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot mark a {Status} notification as sent.");
        }

        Status = NotificationStatus.Sent;
        SentAt = sentAt;
        Error = null;
    }

    public void MarkFailed(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            throw new ArgumentException("An error message is required when marking a notification failed.", nameof(error));
        }

        if (Status != NotificationStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot mark a {Status} notification as failed.");
        }

        Status = NotificationStatus.Failed;
        Error = error.Trim();
    }
}
