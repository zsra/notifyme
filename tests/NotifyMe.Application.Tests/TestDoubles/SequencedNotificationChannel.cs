using NotifyMe.Domain.Abstractions;

namespace NotifyMe.Application.Tests.TestDoubles;

/// <summary>
/// A channel double that returns a different <see cref="NotificationDispatchResult"/> on each
/// successive call (repeating the last one once the sequence is exhausted), used to exercise
/// <c>DispatchNotificationUseCase</c>'s Polly retry pipeline (Phase 09).
/// </summary>
public sealed class SequencedNotificationChannel : INotificationChannel
{
    private readonly NotificationDispatchResult[] _results;

    public SequencedNotificationChannel(string channelType, params NotificationDispatchResult[] results)
    {
        ChannelType = channelType;
        _results = results;
    }

    public string ChannelType { get; }

    public int SendCallCount { get; private set; }

    public Task<NotificationDispatchResult> SendAsync(NotificationDispatchContext context, CancellationToken cancellationToken)
    {
        var result = _results[Math.Min(SendCallCount, _results.Length - 1)];
        SendCallCount++;
        return Task.FromResult(result);
    }
}
