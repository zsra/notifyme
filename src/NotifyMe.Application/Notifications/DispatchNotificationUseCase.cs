using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Application.Common.Exceptions;
using NotifyMe.Domain.Abstractions;
using NotifyMe.Domain.Alerts;
using NotifyMe.Domain.Events;
using NotifyMe.Domain.Notifications;

namespace NotifyMe.Application.Notifications;

/// <summary>
/// Records a <see cref="Notification"/> attempt for one (matched rule, event, channel) tuple and
/// sends it through whichever <see cref="INotificationChannel"/> matches the channel's
/// <c>ChannelType</c>. Every outcome (success, send failure, disabled channel, unknown channel
/// type) is persisted as a terminal <see cref="Notification"/> state rather than thrown, so the
/// admin notification history (docs/api/admin-api.md) has a complete audit trail.
/// </summary>
public sealed class DispatchNotificationUseCase
{
    private readonly IChannelConfigRepository _channelConfigRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IReadOnlyDictionary<string, INotificationChannel> _channelsByType;

    public DispatchNotificationUseCase(
        IChannelConfigRepository channelConfigRepository,
        INotificationRepository notificationRepository,
        IEnumerable<INotificationChannel> channels)
    {
        _channelConfigRepository = channelConfigRepository;
        _notificationRepository = notificationRepository;
        _channelsByType = channels.ToDictionary(channel => channel.ChannelType, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<Notification> ExecuteAsync(
        AlertRule rule, NormalizedEvent normalizedEvent, Guid channelConfigId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(normalizedEvent);

        var channelConfig = await _channelConfigRepository.GetByIdAsync(channelConfigId, cancellationToken)
            ?? throw new NotFoundException($"Channel '{channelConfigId}' was not found.");

        var notification = Notification.CreatePending(Guid.NewGuid(), rule.Id, channelConfig.Id, normalizedEvent.Id);
        await _notificationRepository.AddAsync(notification, cancellationToken);

        if (!channelConfig.IsEnabled)
        {
            notification.MarkFailed($"Channel '{channelConfig.Id}' is disabled.");
            await _notificationRepository.UpdateAsync(notification, cancellationToken);
            return notification;
        }

        if (!_channelsByType.TryGetValue(channelConfig.ChannelType, out var channel))
        {
            notification.MarkFailed(
                $"No notification channel implementation registered for channel type '{channelConfig.ChannelType}'.");
            await _notificationRepository.UpdateAsync(notification, cancellationToken);
            return notification;
        }

        var context = new NotificationDispatchContext(rule, normalizedEvent, channelConfig);
        var result = await channel.SendAsync(context, cancellationToken);

        if (result.IsSuccess)
        {
            notification.MarkSent(DateTimeOffset.UtcNow);
        }
        else
        {
            notification.MarkFailed(result.ErrorMessage ?? "Unknown error.");
        }

        await _notificationRepository.UpdateAsync(notification, cancellationToken);
        return notification;
    }
}
