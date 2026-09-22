namespace NotifyMe.Infrastructure.EventSources.Simulated;

/// <summary>
/// Configuration for <see cref="SimulatedEventSource"/>, bound from the
/// <c>EventIngestion:Simulated</c> appsettings section.
/// </summary>
public sealed class SimulatedEventSourceOptions
{
    public const string SectionName = "EventIngestion:Simulated";

    /// <summary>
    /// Seed for the RNG. Null (the default) means non-deterministic, time-based seeding, which
    /// is what an actually-running instance should use. Tests set this to a fixed value to get
    /// deterministic, repeatable output.
    /// </summary>
    public int? Seed { get; set; }

    /// <summary>
    /// How often a caller (the ingestion worker, from Phase 09 onward) is expected to poll this
    /// source. Not enforced by <see cref="SimulatedEventSource"/> itself, which is a pure
    /// "fetch on demand" source; the polling loop is the worker's responsibility.
    /// </summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Inclusive lower bound on how many events a single <see cref="SimulatedEventSource.FetchAsync"/>
    /// call produces.
    /// </summary>
    public int MinEventsPerFetch { get; set; } = 1;

    /// <summary>
    /// Inclusive upper bound on how many events a single <see cref="SimulatedEventSource.FetchAsync"/>
    /// call produces.
    /// </summary>
    public int MaxEventsPerFetch { get; set; } = 3;
}
