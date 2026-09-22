# ADR-0004: Simulated event source behind a visible extensibility seam

- **Status**: Accepted
- **Date**: 2026-09-22

## Context
The brief never defines what "something important" means, where event data comes from, or how
events are detected. Given the project's time budget, integrating multiple real external APIs
(news, market data, disaster feeds) risks burning most of the available time on API keys, rate
limits, and auth, for a part of the system that isn't the point being evaluated. At the same
time, the brief explicitly asks for flexibility to extend later, so the ingestion boundary needs
to be a real, visible seam, not just an assumption.

## Decision
Implement event ingestion as a fully simulated generator (`SimulatedEventSource`) behind an
`IEventSource` interface defined in `Application`. Isolate the simulator in its own
`Infrastructure/EventSources/Simulated` namespace, and add a sibling
`Infrastructure/EventSources/External` folder containing only a README that documents precisely
how a real source would be implemented and registered, so the extension point is discoverable in
the codebase itself and not just described in prose.

## Alternatives considered
- Hybrid: one real, no-auth-required source (e.g. a public earthquake feed) plus a mock/manual
  source - more impressive end-to-end, but adds an external dependency and failure mode
  (availability, schema drift) to a part of the system that's explicitly a stand-in per the
  brief's own admission that "how events are detected" is undefined.
- Multiple real external APIs (news, market, disaster) - most realistic, but the highest time
  cost for the lowest evaluation signal given the stated focus on process and design judgment
  over feature completeness.

## Consequences
The ingestion pipeline is fully deterministic and demoable without any external dependency or
API key. The `IEventSource` boundary must be kept strictly clean (no simulator-specific types
leaking into `Domain`/`Application`) so that swapping in a real source later is genuinely a
drop-in change, not just a documented aspiration. This is a known, explicit scope limitation.
