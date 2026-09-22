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

    /// <summary>
    /// <c>null</c> means admin/global-owned (unchanged since Phase 08). A non-null value is the
    /// <see cref="Users.User"/> who owns this subscription via the Phase 16 self-service
    /// `/api/me` surface; see ADR-0010.
    /// </summary>
    public Guid? OwnerUserId { get; private set; }

    private Subscription()
    {
    }

    private Subscription(Guid id, Guid alertRuleId, Guid channelConfigId, Guid? ownerUserId)
        : base(id)
    {
        AlertRuleId = alertRuleId;
        ChannelConfigId = channelConfigId;
        OwnerUserId = ownerUserId;
    }

    public static Subscription Create(Guid id, Guid alertRuleId, Guid channelConfigId, Guid? ownerUserId = null)
    {
        if (alertRuleId == Guid.Empty)
        {
            throw new ArgumentException("Subscription must reference an alert rule.", nameof(alertRuleId));
        }

        if (channelConfigId == Guid.Empty)
        {
            throw new ArgumentException("Subscription must reference a channel.", nameof(channelConfigId));
        }

        return new Subscription(id, alertRuleId, channelConfigId, ownerUserId);
    }
}
