# Phase 06 - Infrastructure: event ingestion (simulated, visible cut)

## Goal
Implement event ingestion as a fully simulated generator, but built behind a clearly visible
seam so a real event source (news/market/disaster feed) can be swapped in later without
touching Domain or Application code.

## Depends on
Phase 04 (`IEventSource` interface must exist). Can be worked in parallel with Phase 07.

## Steps
- [ ] Implement `SimulatedEventSource : IEventSource` under
      `Infrastructure/EventSources/Simulated/`, generating plausible events across all three
      `EventCategory` values, using a seeded RNG for deterministic test output.
- [ ] Add a sibling folder `Infrastructure/EventSources/External/README.md` documenting exactly
      how a real source would implement `IEventSource` and be registered via DI, so the seam is
      discoverable in the codebase itself, not just in docs.
- [ ] Make the polling interval and event pool configurable via `appsettings`.
- [ ] Unit tests asserting deterministic output given a fixed seed, and coverage of all three
      event categories being produced over enough iterations.

## Verification
- `SimulatedEventSource` lives in its own namespace, isolated from `Domain`/`Application`.
- `docs/architecture/event-ingestion.md` accurately describes the implemented code (cross-check
  and correct either side if they've drifted).
- Unit tests pass and are deterministic (no flaky random-seed issues).

## Status
Not started.
