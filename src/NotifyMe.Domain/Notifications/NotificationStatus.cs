namespace NotifyMe.Domain.Notifications;

/// <summary>
/// Lifecycle of a single dispatch attempt. One-way transitions out of <see cref="Pending"/>;
/// see <see cref="Notification"/> for the state machine that enforces this.
/// </summary>
public enum NotificationStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2,
}
