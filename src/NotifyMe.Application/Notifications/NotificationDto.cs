using NotifyMe.Domain.Notifications;

namespace NotifyMe.Application.Notifications;

public sealed record NotificationDto(
    Guid Id,
    Guid AlertRuleId,
    Guid ChannelConfigId,
    Guid NormalizedEventId,
    NotificationStatus Status,
    DateTimeOffset? SentAt,
    string? Error)
{
    public static NotificationDto FromEntity(Notification notification) => new(
        notification.Id,
        notification.AlertRuleId,
        notification.ChannelConfigId,
        notification.NormalizedEventId,
        notification.Status,
        notification.SentAt,
        notification.Error);
}
