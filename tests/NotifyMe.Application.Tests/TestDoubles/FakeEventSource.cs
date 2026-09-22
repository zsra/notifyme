using NotifyMe.Domain.Abstractions;
using NotifyMe.Domain.Events;

namespace NotifyMe.Application.Tests.TestDoubles;

public sealed class FakeEventSource : IEventSource
{
    private readonly IReadOnlyList<RawEvent> _rawEvents;

    public FakeEventSource(params RawEvent[] rawEvents)
    {
        _rawEvents = rawEvents;
    }

    public Task<IReadOnlyList<RawEvent>> FetchAsync(CancellationToken cancellationToken) =>
        Task.FromResult(_rawEvents);
}
