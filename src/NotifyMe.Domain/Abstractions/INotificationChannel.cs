namespace NotifyMe.Domain.Abstractions;

/// <summary>
/// The channel abstraction: Slack, Email, and any future channel all implement this. See
/// docs/architecture/notification-channels.md for why <see cref="ChannelType"/> is a
/// string discriminator instead of a closed enum.
/// </summary>
public interface INotificationChannel
{
    /// <summary>
    /// Must match the <c>ChannelConfig.ChannelType</c> value (e.g. <c>"slack"</c>, <c>"email"</c>)
    /// this implementation knows how to send through.
    /// </summary>
    string ChannelType { get; }

    Task<NotificationDispatchResult> SendAsync(NotificationDispatchContext context, CancellationToken cancellationToken);
}
