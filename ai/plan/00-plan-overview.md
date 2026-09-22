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
- [x] [Phase 04 - Application layer](phase-04-application-layer.md)
- [x] [Phase 05 - Infrastructure: persistence](phase-05-infrastructure-persistence.md)
- [x] [Phase 06 - Infrastructure: event ingestion (simulated, visible cut)](phase-06-infrastructure-event-ingestion.md)
- [x] [Phase 07 - Infrastructure: notification channels](phase-07-infrastructure-notification-channels.md)
- [x] [Phase 08 - API layer](phase-08-api-layer.md)
- [ ] [Phase 09 - Background workers](phase-09-background-workers.md)
- [ ] [Phase 10 - Cross-cutting concerns](phase-10-cross-cutting-concerns.md)
- [ ] [Phase 11 - Testing hardening](phase-11-testing-hardening.md)
- [ ] [Phase 12 - CI & repo polish](phase-12-ci-and-repo-polish.md)
- [ ] Frontend - deferred, to be planned separately once Phase 08 is stable

## Sequencing notes

Phases 00-05 and 08-12 are sequential. Phases 06 and 07 both depend on Phase 04 (the interfaces
they implement) but not on each other, so they can be worked in either order or in parallel.

## Current status

Phases 00-08 done. Domain and Application layers are implemented and unit tested; Infrastructure
has real (EF Core/PostgreSQL) repositories, a deterministic `SimulatedEventSource`, and real
Slack (webhook)/Email (SMTP via MailKit) notification channels, both verified end-to-end locally
(WireMock.Net for Slack, MailHog for Email). The Admin API (Phase 08) now composes all of this via
DI: minimal-API CRUD for AlertRules/Channels/Subscriptions, read-only Notification history, a
manual trigger-simulated-event endpoint, API-key auth (`IEndpointFilter`), `ProblemDetails` error
mapping (`IExceptionHandler`), and OpenAPI generation - 102 tests passing total across unit and
integration suites. Phase 09 (Background workers) is next.
