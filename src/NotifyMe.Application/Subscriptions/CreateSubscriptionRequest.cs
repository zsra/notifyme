namespace NotifyMe.Application.Subscriptions;

public sealed record CreateSubscriptionRequest(Guid AlertRuleId, Guid ChannelConfigId);
