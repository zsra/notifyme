using NotifyMe.Domain.Events;

namespace NotifyMe.Domain.Abstractions;

/// <summary>
/// The event-ingestion seam. Implemented today by a fully simulated source in
/// <c>NotifyMe.Infrastructure/EventSources/Simulated</c>; a real source (news/market/disaster
/// feed) plugs in by implementing this interface with no changes to Domain or Application. See
/// docs/architecture/event-ingestion.md.
/// </summary>
public interface IEventSource
{
    Task<IReadOnlyList<RawEvent>> FetchAsync(CancellationToken cancellationToken);
}
