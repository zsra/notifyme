using NotifyMe.Domain.Abstractions;

namespace NotifyMe.Application.Tests.TestDoubles;

public sealed class FakeNotificationChannel : INotificationChannel
{
    private readonly NotificationDispatchResult _result;

    public FakeNotificationChannel(string channelType, NotificationDispatchResult result)
    {
        ChannelType = channelType;
        _result = result;
    }

    public string ChannelType { get; }

    public int SendCallCount { get; private set; }

    public Task<NotificationDispatchResult> SendAsync(NotificationDispatchContext context, CancellationToken cancellationToken)
    {
        SendCallCount++;
        return Task.FromResult(_result);
    }
}
