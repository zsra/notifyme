using NotifyMe.Domain.Common;

namespace NotifyMe.Domain.Subscriptions;

/// <summary>
/// Links an <see cref="Alerts.AlertRule"/> to a <see cref="Channels.ChannelConfig"/> it should
/// notify through when the rule matches.
/// </summary>
public sealed class Subscription : Entity
{
    public Guid AlertRuleId { get; private set; }
    public Guid ChannelConfigId { get; private set; }

    private Subscription()
    {
    }

    private Subscription(Guid id, Guid alertRuleId, Guid channelConfigId)
        : base(id)
    {
        AlertRuleId = alertRuleId;
        ChannelConfigId = channelConfigId;
    }

    public static Subscription Create(Guid id, Guid alertRuleId, Guid channelConfigId)
    {
        if (alertRuleId == Guid.Empty)
        {
            throw new ArgumentException("Subscription must reference an alert rule.", nameof(alertRuleId));
        }

        if (channelConfigId == Guid.Empty)
        {
            throw new ArgumentException("Subscription must reference a channel.", nameof(channelConfigId));
        }

        return new Subscription(id, alertRuleId, channelConfigId);
    }
}
