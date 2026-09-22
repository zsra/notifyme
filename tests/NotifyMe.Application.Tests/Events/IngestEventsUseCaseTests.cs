using NotifyMe.Application.Alerts;
using NotifyMe.Application.Events;
using NotifyMe.Application.Notifications;
using NotifyMe.Application.Tests.TestDoubles;
using NotifyMe.Domain.Abstractions;
using NotifyMe.Domain.Alerts;
using NotifyMe.Domain.Channels;
using NotifyMe.Domain.Common;
using NotifyMe.Domain.Events;
using NotifyMe.Domain.Subscriptions;
using Xunit;

namespace NotifyMe.Application.Tests.Events;

public class IngestEventsUseCaseTests
{
    private static RawEvent MakeRawEvent(EventCategory category, string payload) =>
        RawEvent.Create(Guid.NewGuid(), "simulated", category, payload, DateTimeOffset.UtcNow);

    [Fact]
    public async Task ExecuteAsync_MatchingRuleWithSubscription_DispatchesNotification()
    {
        var alertRules = new InMemoryAlertRuleRepository();
        var subscriptions = new InMemorySubscriptionRepository();
        var channels = new InMemoryChannelConfigRepository();
        var notifications = new InMemoryNotificationRepository();

        var rule = AlertRule.Create(
            Guid.NewGuid(), "Rule", EventCategory.NaturalDisaster,
            MatchCriteria.Create(new[] { "earthquake" }, Severity.Low), DateTimeOffset.UtcNow);
        var channelConfig = ChannelConfig.Create(Guid.NewGuid(), "slack", "https://hooks.example.com/PLACEHOLDER");
        var subscription = Subscription.Create(Guid.NewGuid(), rule.Id, channelConfig.Id);

        alertRules.Seed(rule);
        channels.Seed(channelConfig);
        subscriptions.Seed(subscription);

        var fakeChannel = new FakeNotificationChannel("slack", NotificationDispatchResult.Success());
        var dispatchUseCase = new DispatchNotificationUseCase(channels, notifications, new[] { fakeChannel });
        var evaluateService = new EvaluateAlertRulesService(alertRules, new AlertMatcher());
        var eventSource = new FakeEventSource(MakeRawEvent(EventCategory.NaturalDisaster, "Major earthquake strikes region"));

        var useCase = new IngestEventsUseCase(eventSource, evaluateService, subscriptions, dispatchUseCase);

        var result = await useCase.ExecuteAsync(CancellationToken.None);

        Assert.Equal(1, result.RawEventCount);
        Assert.Equal(1, result.NormalizedEventCount);
        Assert.Equal(1, result.NotificationsDispatched);
        Assert.Equal(1, fakeChannel.SendCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_NoMatchingRule_DispatchesNothing()
    {
        var alertRules = new InMemoryAlertRuleRepository();
        var subscriptions = new InMemorySubscriptionRepository();
        var channels = new InMemoryChannelConfigRepository();
        var notifications = new InMemoryNotificationRepository();

        var rule = AlertRule.Create(
            Guid.NewGuid(), "Rule", EventCategory.MarketMovement,
            MatchCriteria.Create(new[] { "crash" }, Severity.Low), DateTimeOffset.UtcNow);
        alertRules.Seed(rule);

        var dispatchUseCase = new DispatchNotificationUseCase(channels, notifications, Array.Empty<FakeNotificationChannel>());
        var evaluateService = new EvaluateAlertRulesService(alertRules, new AlertMatcher());
        var eventSource = new FakeEventSource(MakeRawEvent(EventCategory.BreakingNews, "Unrelated news"));

        var useCase = new IngestEventsUseCase(eventSource, evaluateService, subscriptions, dispatchUseCase);

        var result = await useCase.ExecuteAsync(CancellationToken.None);

        Assert.Equal(1, result.RawEventCount);
        Assert.Equal(0, result.NotificationsDispatched);
    }

    [Fact]
    public async Task ExecuteAsync_MatchingRuleWithNoSubscriptions_DispatchesNothing()
    {
        var alertRules = new InMemoryAlertRuleRepository();
        var subscriptions = new InMemorySubscriptionRepository();
        var channels = new InMemoryChannelConfigRepository();
        var notifications = new InMemoryNotificationRepository();

        var rule = AlertRule.Create(
            Guid.NewGuid(), "Rule", EventCategory.NaturalDisaster,
            MatchCriteria.Create(null, Severity.Medium), DateTimeOffset.UtcNow);
        alertRules.Seed(rule);

        var dispatchUseCase = new DispatchNotificationUseCase(channels, notifications, Array.Empty<FakeNotificationChannel>());
        var evaluateService = new EvaluateAlertRulesService(alertRules, new AlertMatcher());
        var eventSource = new FakeEventSource(MakeRawEvent(EventCategory.NaturalDisaster, "Flood warning issued"));

        var useCase = new IngestEventsUseCase(eventSource, evaluateService, subscriptions, dispatchUseCase);

        var result = await useCase.ExecuteAsync(CancellationToken.None);

        Assert.Equal(0, result.NotificationsDispatched);
    }
}
