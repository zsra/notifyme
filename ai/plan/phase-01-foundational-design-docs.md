# Phase 01 - Foundational design docs & ADRs

## Goal
Do the up-front design thinking and write it down *before* any code exists: architecture
overview, the event-ingestion "visible cut", the notification-channel abstraction, the data
model, and a draft Admin API contract. Also formalize the decisions already made during
interactive planning as ADRs.

## Depends on
Phase 00 (folders/templates must exist).

## Steps
- [x] Write ADR-0001 through ADR-0008 (one per confirmed decision: .NET version, architecture
      style, database choice, event-source strategy, notification channels, admin API auth,
      testing strategy, AI-friendly scaffolding) using `ai/decisions/adr/template.md`.
- [x] Write `docs/architecture/overview.md`: system context, component diagram (mermaid),
      narrative description of the Clean Architecture layering.
- [x] Write `docs/architecture/event-ingestion.md`: the `IEventSource` abstraction, the
      simulated implementation, and exactly how a real source would plug in later.
- [x] Write `docs/architecture/notification-channels.md`: the `INotificationChannel`
      abstraction, Slack/Email implementations, and how to add a new channel without touching
      core domain types.
- [x] Write `docs/architecture/data-model.md`: entities, relationships, and an ER diagram
      (mermaid).
- [x] Write `docs/api/admin-api.md`: draft endpoint list, DTO shapes, auth header contract.
- [x] Write `docs/runbook.md` as a draft/TBD placeholder (will be filled in once code exists).

## Verification
- Every ADR has Context / Decision / Alternatives Considered / Consequences.
- The design docs are internally consistent with each other (same entity names, same
  abstraction names) so that Phase 02+ code can be written straight from them.
- User has reviewed and no open contradictions remain before Phase 02 starts.

## Status
Done (2026-09-22). Treat as a first draft, open to revision as implementation surfaces new
details; no contradictions flagged before starting Phase 02.
