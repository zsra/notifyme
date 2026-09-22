# Event ingestion: the simulated source and its extension seam

## Why simulated

The brief never defines where event data comes from or how "something important" is detected.
Rather than guess and integrate real external APIs under time pressure (see ADR-0004 for the
full reasoning), event ingestion is implemented as a deterministic simulator. The important part
architecturally is that this is a clean, swappable boundary, not a shortcut baked into the rest
of the system.

## The abstraction

`IEventSource` is declared in the `Domain` layer, under `Domain/Abstractions`:

```
Task<IReadOnlyList<RawEvent>> FetchAsync(CancellationToken ct);
```

Nothing in `Domain` or `Application` knows or cares whether events come from a simulator or a
real feed. `EventIngestionWorker` (in `Api/Workers/`, added in Phase 09) depends only on
`IEventSource`, resolved via DI.

## The simulated implementation

`SimulatedEventSource` (in `Infrastructure/EventSources/Simulated/`) generates plausible
`RawEvent`s spanning all three categories (`BreakingNews`, `MarketMovement`, `NaturalDisaster`)
from a fixed content pool, using a seedable RNG (`SimulatedEventSourceOptions.Seed`) so behavior
is deterministic and testable when a seed is supplied; left unset, it seeds from the clock like a
normal `Random`. How many events a single `FetchAsync` call returns, and the polling interval a
caller should use, are both configurable via the `EventIngestion:Simulated` appsettings section
(`MinEventsPerFetch`, `MaxEventsPerFetch`, `PollingInterval`) - see
`src/NotifyMe.Api/appsettings.json`. `SimulatedEventSource` itself only fetches on demand;
`EventIngestionWorker` (Phase 09) owns the actual polling loop, running one pass immediately on
startup and then repeating on `PollingInterval`.

Registration lives in `Infrastructure/EventSources/EventSourcesServiceCollectionExtensions.cs`
(`AddSimulatedEventSource`), called from the `Api` composition root (from Phase 08 onward).


## The extension seam

A sibling folder, `Infrastructure/EventSources/External/`, exists specifically to make the
extension point discoverable in the codebase itself rather than only described here. It documents
what a real implementation would need to do:

1. Implement `IEventSource` (e.g. `NewsApiEventSource`, `EarthquakeFeedEventSource`,
   `MarketDataEventSource`).
2. Map the external payload into the same `RawEvent` shape the rest of the pipeline expects.
3. Register the new implementation in DI in place of (or alongside) `SimulatedEventSource`,
   entirely within `Infrastructure` and the `Api` composition root.

No changes to `Domain`, `Application`, the matching logic, or the notification channels should be
required to add a real source later. If a future change requires touching those layers to add a
source, that's a signal the abstraction has leaked and should be revisited.

## Known limitation

This is an explicit, documented scope cut (see ADR-0004): there is currently no real external
event data in the system, only simulated events. This is by design for this phase, not an
oversight.
