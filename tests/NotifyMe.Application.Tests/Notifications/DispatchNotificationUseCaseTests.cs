using NotifyMe.Application.Common.Exceptions;
using NotifyMe.Application.Notifications;
using NotifyMe.Application.Tests.TestDoubles;
using NotifyMe.Domain.Abstractions;
using NotifyMe.Domain.Alerts;
using NotifyMe.Domain.Channels;
using NotifyMe.Domain.Common;
using NotifyMe.Domain.Events;
using NotifyMe.Domain.Notifications;
using Polly;
using Polly.Retry;
using Xunit;

namespace NotifyMe.Application.Tests.Notifications;

public class DispatchNotificationUseCaseTests
{
    private static ResiliencePipeline<NotificationDispatchResult> MakeRetryPipeline(int maxRetryAttempts) =>
        new ResiliencePipelineBuilder<NotificationDispatchResult>()
            .AddRetry(new RetryStrategyOptions<NotificationDispatchResult>
            {
                ShouldHandle = new PredicateBuilder<NotificationDispatchResult>().HandleResult(result => !result.IsSuccess),
                MaxRetryAttempts = maxRetryAttempts,
                Delay = TimeSpan.Zero,
            })
            .Build();

    private static AlertRule MakeRule() => AlertRule.Create(
        Guid.NewGuid(), "Rule", EventCategory.BreakingNews, MatchCriteria.Create(null, Severity.Medium), DateTimeOffset.UtcNow);

    private static NormalizedEvent MakeEvent() => NormalizedEvent.Create(
        Guid.NewGuid(), Guid.NewGuid(), EventCategory.BreakingNews, "Title", null, Severity.High, DateTimeOffset.UtcNow);

    [Fact]
    public async Task ExecuteAsync_SuccessfulSend_MarksNotificationSent()
    {
        var channels = new InMemoryChannelConfigRepository();
        var notifications = new InMemoryNotificationRepository();
        var channelConfig = ChannelConfig.Create(Guid.NewGuid(), "slack", "https://hooks.example.com/PLACEHOLDER");
        channels.Seed(channelConfig);
        var fakeChannel = new FakeNotificationChannel("slack", NotificationDispatchResult.Success());

        var useCase = new DispatchNotificationUseCase(channels, notifications, new[] { fakeChannel });

        var notification = await useCase.ExecuteAsync(MakeRule(), MakeEvent(), channelConfig.Id, CancellationToken.None);

        Assert.Equal(NotificationStatus.Sent, notification.Status);
        Assert.Equal(1, fakeChannel.SendCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ChannelReturnsFailure_MarksNotificationFailedWithError()
    {
        var channels = new InMemoryChannelConfigRepository();
        var notifications = new InMemoryNotificationRepository();
        var channelConfig = ChannelConfig.Create(Guid.NewGuid(), "email", "ops@example.com");
        channels.Seed(channelConfig);
        var fakeChannel = new FakeNotificationChannel("email", NotificationDispatchResult.Failure("SMTP timeout"));

        var useCase = new DispatchNotificationUseCase(channels, notifications, new[] { fakeChannel });

        var notification = await useCase.ExecuteAsync(MakeRule(), MakeEvent(), channelConfig.Id, CancellationToken.None);

        Assert.Equal(NotificationStatus.Failed, notification.Status);
        Assert.Equal("SMTP timeout", notification.Error);
    }

    [Fact]
    public async Task ExecuteAsync_UnknownChannelType_MarksNotificationFailedWithoutCallingAnyChannel()
    {
        var channels = new InMemoryChannelConfigRepository();
        var notifications = new InMemoryNotificationRepository();
        var channelConfig = ChannelConfig.Create(Guid.NewGuid(), "webhook", "https://example.com/PLACEHOLDER");
        channels.Seed(channelConfig);
        var fakeChannel = new FakeNotificationChannel("slack", NotificationDispatchResult.Success());

        var useCase = new DispatchNotificationUseCase(channels, notifications, new[] { fakeChannel });

        var notification = await useCase.ExecuteAsync(MakeRule(), MakeEvent(), channelConfig.Id, CancellationToken.None);

        Assert.Equal(NotificationStatus.Failed, notification.Status);
        Assert.Equal(0, fakeChannel.SendCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_DisabledChannel_MarksNotificationFailedWithoutCallingChannel()
    {
        var channels = new InMemoryChannelConfigRepository();
        var notifications = new InMemoryNotificationRepository();
        var channelConfig = ChannelConfig.Create(Guid.NewGuid(), "slack", "https://hooks.example.com/PLACEHOLDER");
        channelConfig.Disable();
        channels.Seed(channelConfig);
        var fakeChannel = new FakeNotificationChannel("slack", NotificationDispatchResult.Success());

        var useCase = new DispatchNotificationUseCase(channels, notifications, new[] { fakeChannel });

        var notification = await useCase.ExecuteAsync(MakeRule(), MakeEvent(), channelConfig.Id, CancellationToken.None);

        Assert.Equal(NotificationStatus.Failed, notification.Status);
        Assert.Equal(0, fakeChannel.SendCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_UnknownChannelId_ThrowsNotFoundException()
    {
        var channels = new InMemoryChannelConfigRepository();
        var notifications = new InMemoryNotificationRepository();
        var useCase = new DispatchNotificationUseCase(channels, notifications, Array.Empty<FakeNotificationChannel>());

        await Assert.ThrowsAsync<NotFoundException>(
            () => useCase.ExecuteAsync(MakeRule(), MakeEvent(), Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_TransientFailureThenSuccess_RetriesAndMarksNotificationSent()
    {
        var channels = new InMemoryChannelConfigRepository();
        var notifications = new InMemoryNotificationRepository();
        var channelConfig = ChannelConfig.Create(Guid.NewGuid(), "slack", "https://hooks.example.com/PLACEHOLDER");
        channels.Seed(channelConfig);
        var flakyChannel = new SequencedNotificationChannel(
            "slack",
            NotificationDispatchResult.Failure("transient error"),
            NotificationDispatchResult.Failure("transient error"),
            NotificationDispatchResult.Success());

        var useCase = new DispatchNotificationUseCase(
            channels, notifications, new[] { flakyChannel }, MakeRetryPipeline(maxRetryAttempts: 3));

        var notification = await useCase.ExecuteAsync(MakeRule(), MakeEvent(), channelConfig.Id, CancellationToken.None);

        Assert.Equal(NotificationStatus.Sent, notification.Status);
        Assert.Equal(3, flakyChannel.SendCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_FailuresExceedMaxRetryAttempts_MarksNotificationFailedAfterExhaustingRetries()
    {
        var channels = new InMemoryChannelConfigRepository();
        var notifications = new InMemoryNotificationRepository();
        var channelConfig = ChannelConfig.Create(Guid.NewGuid(), "slack", "https://hooks.example.com/PLACEHOLDER");
        channels.Seed(channelConfig);
        var flakyChannel = new SequencedNotificationChannel("slack", NotificationDispatchResult.Failure("persistent error"));

        var useCase = new DispatchNotificationUseCase(
            channels, notifications, new[] { flakyChannel }, MakeRetryPipeline(maxRetryAttempts: 2));

        var notification = await useCase.ExecuteAsync(MakeRule(), MakeEvent(), channelConfig.Id, CancellationToken.None);

        Assert.Equal(NotificationStatus.Failed, notification.Status);
        Assert.Equal("persistent error", notification.Error);
        Assert.Equal(3, flakyChannel.SendCallCount); // initial attempt + 2 retries
    }
}
