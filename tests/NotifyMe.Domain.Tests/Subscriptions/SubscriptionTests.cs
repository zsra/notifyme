using NotifyMe.Domain.Subscriptions;
using Xunit;

namespace NotifyMe.Domain.Tests.Subscriptions;

public class SubscriptionTests
{
    [Fact]
    public void Create_WithValidIds_Succeeds()
    {
        var alertRuleId = Guid.NewGuid();
        var channelConfigId = Guid.NewGuid();

        var subscription = Subscription.Create(Guid.NewGuid(), alertRuleId, channelConfigId);

        Assert.Equal(alertRuleId, subscription.AlertRuleId);
        Assert.Equal(channelConfigId, subscription.ChannelConfigId);
    }

    [Fact]
    public void Create_WithEmptyAlertRuleId_Throws()
    {
        var act = () => Subscription.Create(Guid.NewGuid(), Guid.Empty, Guid.NewGuid());

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_WithEmptyChannelConfigId_Throws()
    {
        var act = () => Subscription.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty);

        Assert.Throws<ArgumentException>(act);
    }
}
