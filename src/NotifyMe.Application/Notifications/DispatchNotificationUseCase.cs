using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Application.Common.Exceptions;
using NotifyMe.Domain.Abstractions;
using NotifyMe.Domain.Alerts;
using NotifyMe.Domain.Events;
using NotifyMe.Domain.Notifications;
using Polly;

namespace NotifyMe.Application.Notifications;

/// <summary>
/// Records a <see cref="Notification"/> attempt for one (matched rule, event, channel) tuple and
/// sends it through whichever <see cref="INotificationChannel"/> matches the channel's
/// <c>ChannelType</c>. Every outcome (success, send failure, disabled channel, unknown channel
/// type) is persisted as a terminal <see cref="Notification"/> state rather than thrown, so the
/// admin notification history (docs/api/admin-api.md) has a complete audit trail.
///
/// The actual channel send goes through a Polly <see cref="ResiliencePipeline{TResult}"/>
/// (configured in <see cref="ApplicationServiceCollectionExtensions"/> from the
/// `Resilience:ChannelSend` config section, see ai/plan/phase-09-background-workers.md) so a
/// transient channel failure is retried with backoff rather than immediately recorded as failed.
/// Defaults to a no-op pipeline when none is supplied, so existing unit tests that construct
/// this type directly keep their exact call-count expectations.
/// </summary>
public sealed class DispatchNotificationUseCase
{
    private readonly IChannelConfigRepository _channelConfigRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IReadOnlyDictionary<string, INotificationChannel> _channelsByType;
    private readonly ResiliencePipeline<NotificationDispatchResult> _resiliencePipeline;
    private readonly ILogger<DispatchNotificationUseCase> _logger;

    public DispatchNotificationUseCase(
        IChannelConfigRepository channelConfigRepository,
        INotificationRepository notificationRepository,
        IEnumerable<INotificationChannel> channels,
        ResiliencePipeline<NotificationDispatchResult>? resiliencePipeline = null,
        ILogger<DispatchNotificationUseCase>? logger = null)
    {
        _channelConfigRepository = channelConfigRepository;
        _notificationRepository = notificationRepository;
        _channelsByType = channels.ToDictionary(channel => channel.ChannelType, StringComparer.OrdinalIgnoreCase);
        _resiliencePipeline = resiliencePipeline ?? ResiliencePipeline<NotificationDispatchResult>.Empty;
        _logger = logger ?? NullLogger<DispatchNotificationUseCase>.Instance;
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

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = normalizedEvent.Id,
            ["NotificationId"] = notification.Id,
        });

        if (!channelConfig.IsEnabled)
        {
            _logger.LogWarning("Channel {ChannelConfigId} is disabled; notification recorded as failed.", channelConfig.Id);
            notification.MarkFailed($"Channel '{channelConfig.Id}' is disabled.");
            await _notificationRepository.UpdateAsync(notification, cancellationToken);
            return notification;
        }

        if (!_channelsByType.TryGetValue(channelConfig.ChannelType, out var channel))
        {
            _logger.LogWarning(
                "No notification channel implementation registered for channel type '{ChannelType}'.", channelConfig.ChannelType);
            notification.MarkFailed(
                $"No notification channel implementation registered for channel type '{channelConfig.ChannelType}'.");
            await _notificationRepository.UpdateAsync(notification, cancellationToken);
            return notification;
        }

        var context = new NotificationDispatchContext(rule, normalizedEvent, channelConfig);
        _logger.LogDebug("Dispatching notification via channel type {ChannelType} for rule {RuleId}.", channelConfig.ChannelType, rule.Id);

        var result = await _resiliencePipeline.ExecuteAsync(
            async ct => await channel.SendAsync(context, ct), cancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Notification {NotificationId} sent via channel {ChannelConfigId}.", notification.Id, channelConfig.Id);
            notification.MarkSent(DateTimeOffset.UtcNow);
        }
        else
        {
            _logger.LogError(
                "Notification {NotificationId} failed after retries via channel {ChannelConfigId}: {Error}",
                notification.Id, channelConfig.Id, result.ErrorMessage);
            notification.MarkFailed(result.ErrorMessage ?? "Unknown error.");
        }

        await _notificationRepository.UpdateAsync(notification, cancellationToken);
        return notification;
    }
}
