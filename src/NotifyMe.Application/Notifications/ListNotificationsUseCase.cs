using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Domain.Notifications;

namespace NotifyMe.Application.Notifications;

/// <summary>
/// Lists notification history, optionally filtered by status, alert rule, and/or a
/// <see cref="Notification.SentAt"/> date range (per docs/api/admin-api.md). Filtering happens
/// in-memory over <see cref="INotificationRepository.ListAsync"/>, same rationale as the other
/// list use cases in this project. The date range only ever matches notifications that have
/// actually been sent - <see cref="Notification"/> has no separate "created at" timestamp, only
/// <see cref="Notification.SentAt"/>, so a date-range filter naturally excludes still-pending
/// or failed-before-sending attempts. This is a known, accepted limitation rather than an
/// oversight.
/// </summary>
public sealed class ListNotificationsUseCase
{
    private readonly INotificationRepository _notificationRepository;

    public ListNotificationsUseCase(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<IReadOnlyList<NotificationDto>> ExecuteAsync(
        NotificationStatus? status,
        Guid? alertRuleId,
        DateTimeOffset? sentFrom,
        DateTimeOffset? sentTo,
        CancellationToken cancellationToken)
    {
        var notifications = await _notificationRepository.ListAsync(cancellationToken);

        return notifications
            .Where(notification => status is null || notification.Status == status)
            .Where(notification => alertRuleId is null || notification.AlertRuleId == alertRuleId)
            .Where(notification => sentFrom is null || (notification.SentAt is { } sentAt && sentAt >= sentFrom))
            .Where(notification => sentTo is null || (notification.SentAt is { } sentAt && sentAt <= sentTo))
            .Select(NotificationDto.FromEntity)
            .ToList();
    }
}
