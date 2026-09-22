using NotifyMe.Application.Common.Exceptions;
using NotifyMe.Application.Subscriptions;
using NotifyMe.Application.Tests.TestDoubles;
using NotifyMe.Domain.Alerts;
using NotifyMe.Domain.Channels;
using NotifyMe.Domain.Common;
using NotifyMe.Domain.Events;
using NotifyMe.Domain.Subscriptions;
using Xunit;

namespace NotifyMe.Application.Tests.Subscriptions;

public class ManageSubscriptionUseCaseTests
{
    private static (ManageSubscriptionUseCase UseCase, InMemoryAlertRuleRepository AlertRules, InMemoryChannelConfigRepository Channels, InMemorySubscriptionRepository Subscriptions) CreateUseCase()
    {
        var alertRules = new InMemoryAlertRuleRepository();
        var channels = new InMemoryChannelConfigRepository();
        var subscriptions = new InMemorySubscriptionRepository();
        var useCase = new ManageSubscriptionUseCase(subscriptions, alertRules, channels, new CreateSubscriptionRequestValidator());

        return (useCase, alertRules, channels, subscriptions);
    }

    [Fact]
    public async Task SubscribeAsync_WithExistingRuleAndChannel_CreatesSubscription()
    {
        var (useCase, alertRules, channels, subscriptions) = CreateUseCase();
        var rule = AlertRule.Create(
            Guid.NewGuid(), "Rule", EventCategory.BreakingNews, MatchCriteria.Create(null, Severity.Medium), DateTimeOffset.UtcNow);
        var channel = ChannelConfig.Create(Guid.NewGuid(), "email", "ops@example.com");
        alertRules.Seed(rule);
        channels.Seed(channel);

        var dto = await useCase.SubscribeAsync(new CreateSubscriptionRequest(rule.Id, channel.Id), CancellationToken.None);

        Assert.Equal(rule.Id, dto.AlertRuleId);
        Assert.Equal(channel.Id, dto.ChannelConfigId);
        Assert.Single(await subscriptions.ListAsync(CancellationToken.None));
    }

    [Fact]
    public async Task SubscribeAsync_WithUnknownAlertRule_ThrowsNotFoundException()
    {
        var (useCase, _, channels, _) = CreateUseCase();
        var channel = ChannelConfig.Create(Guid.NewGuid(), "email", "ops@example.com");
        channels.Seed(channel);

        var request = new CreateSubscriptionRequest(Guid.NewGuid(), channel.Id);

        await Assert.ThrowsAsync<NotFoundException>(() => useCase.SubscribeAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task SubscribeAsync_WithUnknownChannel_ThrowsNotFoundException()
    {
        var (useCase, alertRules, _, _) = CreateUseCase();
        var rule = AlertRule.Create(
            Guid.NewGuid(), "Rule", EventCategory.BreakingNews, MatchCriteria.Create(null, Severity.Medium), DateTimeOffset.UtcNow);
        alertRules.Seed(rule);

        var request = new CreateSubscriptionRequest(rule.Id, Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() => useCase.SubscribeAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task UnsubscribeAsync_WithExistingSubscription_RemovesIt()
    {
        var (useCase, _, _, subscriptions) = CreateUseCase();
        var subscription = Subscription.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        subscriptions.Seed(subscription);

        await useCase.UnsubscribeAsync(subscription.Id, CancellationToken.None);

        Assert.Empty(await subscriptions.ListAsync(CancellationToken.None));
    }

    [Fact]
    public async Task UnsubscribeAsync_WithUnknownId_ThrowsNotFoundException()
    {
        var (useCase, _, _, _) = CreateUseCase();

        await Assert.ThrowsAsync<NotFoundException>(() => useCase.UnsubscribeAsync(Guid.NewGuid(), CancellationToken.None));
    }
}
