# Plan overview

Backend-first implementation plan for NotifyMe, built in small, independently verifiable phases.
Each phase has its own file with goal, dependencies, steps, and verification criteria. Update the
checkboxes below as phases complete; keep this file as the single source of truth for status.

Frontend is explicitly out of scope for this plan; it will be scoped separately once the Admin
API contract (end of Phase 8) is stable.

## Phases

- [x] [Phase 00 - Repo & AI-friendly scaffolding](phase-00-repo-scaffolding.md)
- [x] [Phase 01 - Foundational design docs & ADRs](phase-01-foundational-design-docs.md)
- [x] [Phase 02 - Solution & project scaffolding](phase-02-solution-scaffolding.md)
- [x] [Phase 03 - Domain layer](phase-03-domain-layer.md)
- [ ] [Phase 04 - Application layer](phase-04-application-layer.md)
- [ ] [Phase 05 - Infrastructure: persistence](phase-05-infrastructure-persistence.md)
- [ ] [Phase 06 - Infrastructure: event ingestion (simulated, visible cut)](phase-06-infrastructure-event-ingestion.md)
- [ ] [Phase 07 - Infrastructure: notification channels](phase-07-infrastructure-notification-channels.md)
- [ ] [Phase 08 - API layer](phase-08-api-layer.md)
- [ ] [Phase 09 - Background workers](phase-09-background-workers.md)
- [ ] [Phase 10 - Cross-cutting concerns](phase-10-cross-cutting-concerns.md)
- [ ] [Phase 11 - Testing hardening](phase-11-testing-hardening.md)
- [ ] [Phase 12 - CI & repo polish](phase-12-ci-and-repo-polish.md)
- [ ] Frontend - deferred, to be planned separately once Phase 08 is stable

## Sequencing notes

Phases 00-05 and 08-12 are sequential. Phases 06 and 07 both depend on Phase 04 (the interfaces
they implement) but not on each other, so they can be worked in either order or in parallel.

## Current status

Phases 00-03 done. The Domain layer (entities, value objects, and the `IEventSource`/
`INotificationChannel`/`IAlertMatcher` interfaces) is implemented with zero framework
dependencies and 54 passing unit tests. Phase 04 (Application layer) is next.
