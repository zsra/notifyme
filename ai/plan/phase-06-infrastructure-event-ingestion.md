# Phase 06 - Infrastructure: event ingestion (simulated, visible cut)

## Goal
Implement event ingestion as a fully simulated generator, but built behind a clearly visible
seam so a real event source (news/market/disaster feed) can be swapped in later without
touching Domain or Application code.

## Depends on
Phase 04 (`IEventSource` interface must exist). Can be worked in parallel with Phase 07.

## Steps
- [x] Implement `SimulatedEventSource : IEventSource` under
      `Infrastructure/EventSources/Simulated/`, generating plausible events across all three
      `EventCategory` values, using a seeded RNG for deterministic test output.
- [x] Add a sibling folder `Infrastructure/EventSources/External/README.md` documenting exactly
      how a real source would implement `IEventSource` and be registered via DI, so the seam is
      discoverable in the codebase itself, not just in docs.
- [x] Make the polling interval and event pool configurable via `appsettings`.
- [x] Unit tests asserting deterministic output given a fixed seed, and coverage of all three
      event categories being produced over enough iterations.

## Verification
- `SimulatedEventSource` lives in its own namespace, isolated from `Domain`/`Application`.
- `docs/architecture/event-ingestion.md` accurately describes the implemented code (cross-check
  and correct either side if they've drifted).
- Unit tests pass and are deterministic (no flaky random-seed issues).

## Design notes / course-corrections
- Fixed a doc/code drift found while cross-checking: `docs/architecture/event-ingestion.md`
  said `IEventSource` was declared in `Application`; it's actually in `Domain/Abstractions`
  (confirmed against Phase 03/04's own notes, which correctly list it as a Domain interface).
  Corrected the doc rather than moving the interface.
- `SimulatedEventSource` takes `IOptions<SimulatedEventSourceOptions>` (`Seed`,
  `PollingInterval`, `MinEventsPerFetch`, `MaxEventsPerFetch`), bound from the
  `EventIngestion:Simulated` appsettings section via `Microsoft.Extensions.Options.ConfigurationExtensions`
  (added as an explicit package reference on `NotifyMe.Infrastructure`).
- `Seed` is deliberately left unset in `appsettings.json` (defaults to `null` -> time-based
  `Random`) so a real running instance isn't stuck replaying the same sequence; tests supply a
  fixed seed explicitly for determinism.
- `PollingInterval` is stored/configurable now but not yet consumed by anything - there is no
  worker yet (that's Phase 09). `SimulatedEventSource` itself is a pure "fetch on demand" source;
  polling is a future caller's responsibility.
- Content pool: a small fixed set of plausible source/payload pairs per `EventCategory`
  (4 each), picked via the seeded `Random` alongside a random category and a random count in
  `[MinEventsPerFetch, MaxEventsPerFetch]` per `FetchAsync` call.
- DI registration (`AddSimulatedEventSource`) lives in
  `Infrastructure/EventSources/EventSourcesServiceCollectionExtensions.cs`, mirroring the
  `AddNotifyMePersistence` pattern from Phase 05; intended to be called from Phase 08's Api
  composition root.
- New test project `NotifyMe.Infrastructure.Tests` was added (didn't exist before this phase),
  referencing `NotifyMe.Infrastructure` and `NotifyMe.Domain`, registered in `NotifyMe.slnx`.

## Status
Done (2026-09-22).
