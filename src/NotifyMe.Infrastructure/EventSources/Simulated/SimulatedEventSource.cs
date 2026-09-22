using Microsoft.Extensions.Options;
using NotifyMe.Domain.Abstractions;
using NotifyMe.Domain.Events;

namespace NotifyMe.Infrastructure.EventSources.Simulated;

/// <summary>
/// Fully simulated <see cref="IEventSource"/>: generates plausible <see cref="RawEvent"/>s
/// spanning all three <see cref="EventCategory"/> values from a fixed content pool, using a
/// seedable RNG so behavior is deterministic and testable. See
/// docs/architecture/event-ingestion.md and ADR-0004 for why this exists and how the extension
/// seam to a real source works.
///
/// This type intentionally knows nothing about Domain/Application beyond the
/// <see cref="IEventSource"/> contract and the <see cref="RawEvent"/> shape it must produce, so
/// it stays isolated from the rest of the pipeline.
/// </summary>
public sealed class SimulatedEventSource : IEventSource
{
    private static readonly IReadOnlyList<EventCategory> Categories = Enum.GetValues<EventCategory>();

    private static readonly IReadOnlyDictionary<EventCategory, IReadOnlyList<(string Source, string Payload)>> Pool =
        new Dictionary<EventCategory, IReadOnlyList<(string Source, string Payload)>>
        {
            [EventCategory.BreakingNews] = new[]
            {
                ("global-wire-news", "Major transit strike shuts down downtown commuter rail lines"),
                ("global-wire-news", "Government announces surprise cabinet reshuffle"),
                ("global-wire-news", "Large-scale power outage reported across several city districts"),
                ("global-wire-news", "Airport ground stop issued after security incident"),
            },
            [EventCategory.MarketMovement] = new[]
            {
                ("market-data-feed", "Benchmark index falls 4% amid broad sell-off"),
                ("market-data-feed", "Tech sector rallies on stronger-than-expected earnings"),
                ("market-data-feed", "Central bank signals unexpected policy rate change"),
                ("market-data-feed", "Currency markets turn volatile after trade policy announcement"),
            },
            [EventCategory.NaturalDisaster] = new[]
            {
                ("usgs-earthquake-feed", "Magnitude 6.1 earthquake recorded offshore, tsunami watch issued"),
                ("weather-alert-feed", "Category 3 hurricane expected to make landfall within 24 hours"),
                ("weather-alert-feed", "Flash flood warning issued for multiple river basins"),
                ("wildfire-tracker-feed", "Wildfire has grown to over 10,000 acres, evacuations ordered"),
            },
        };

    private readonly Random _random;
    private readonly SimulatedEventSourceOptions _options;

    public SimulatedEventSource(IOptions<SimulatedEventSourceOptions> options)
    {
        _options = options.Value;
        _random = _options.Seed is { } seed ? new Random(seed) : new Random();
    }

    public Task<IReadOnlyList<RawEvent>> FetchAsync(CancellationToken cancellationToken)
    {
        var count = _random.Next(_options.MinEventsPerFetch, _options.MaxEventsPerFetch + 1);
        var events = new List<RawEvent>(count);

        for (var i = 0; i < count; i++)
        {
            var category = Categories[_random.Next(Categories.Count)];
            var candidates = Pool[category];
            var (source, payload) = candidates[_random.Next(candidates.Count)];

            events.Add(RawEvent.Create(Guid.NewGuid(), source, category, payload, DateTimeOffset.UtcNow));
        }

        return Task.FromResult<IReadOnlyList<RawEvent>>(events);
    }
}
