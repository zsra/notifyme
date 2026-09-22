using NotifyMe.Domain.Subscriptions;

namespace NotifyMe.Application.Subscriptions;

public sealed record SubscriptionDto(Guid Id, Guid AlertRuleId, Guid ChannelConfigId)
{
    public static SubscriptionDto FromEntity(Subscription subscription) => new(
        subscription.Id,
        subscription.AlertRuleId,
        subscription.ChannelConfigId);
}
