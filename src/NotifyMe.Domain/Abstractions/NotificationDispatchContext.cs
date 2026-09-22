using NotifyMe.Domain.Alerts;
using NotifyMe.Domain.Channels;
using NotifyMe.Domain.Events;

namespace NotifyMe.Domain.Abstractions;

/// <summary>
/// Everything an <see cref="INotificationChannel"/> needs to render and send one notification.
/// </summary>
public sealed record NotificationDispatchContext(AlertRule Rule, NormalizedEvent Event, ChannelConfig Channel);

/// <summary>
/// The outcome of one send attempt. Deliberately not a bool: callers (e.g. the dispatch use
/// case in Application, added in Phase 04) need the error message to record on the
/// <see cref="Notifications.Notification"/> when a send fails.
/// </summary>
public sealed record NotificationDispatchResult(bool IsSuccess, string? ErrorMessage)
{
    public static NotificationDispatchResult Success() => new(true, null);

    public static NotificationDispatchResult Failure(string errorMessage) => new(false, errorMessage);
}
