# Plan overview

Backend-first implementation plan for NotifyMe, built in small, independently verifiable phases.
Each phase has its own file with goal, dependencies, steps, and verification criteria. Update the
checkboxes below as phases complete; keep this file as the single source of truth for status.

Frontend was explicitly out of scope until the Admin API contract (end of Phase 8) was stable;
it is now scoped as Phases 13-15 (see ADR-0009).

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
- [x] [Phase 09 - Background workers](phase-09-background-workers.md)
- [x] [Phase 10 - Cross-cutting concerns](phase-10-cross-cutting-concerns.md)
- [x] [Phase 11 - Testing hardening](phase-11-testing-hardening.md)
- [x] [Phase 12 - CI & repo polish](phase-12-ci-and-repo-polish.md)
- [x] [Phase 13 - Frontend foundation & API integration](phase-13-frontend-foundation.md)
- [x] [Phase 14 - Frontend admin screens](phase-14-frontend-admin-screens.md)
- [x] [Phase 15 - Frontend testing, CI, and docs](phase-15-frontend-testing-and-docs.md)

## Sequencing notes

Phases 00-05 and 08-15 are sequential. Phases 06 and 07 both depend on Phase 04 (the interfaces
they implement) but not on each other, so they can be worked in either order or in parallel.
Phases 13-15 (frontend) only depend on Phase 08 (the Admin API contract); they were deferred
until that contract was stable but are otherwise sequential among themselves.

## Current status

Phases 00-11 done. Domain and Application layers are implemented and unit tested; Infrastructure
has real (EF Core/PostgreSQL) repositories, a deterministic `SimulatedEventSource`, and real
Slack (webhook)/Email (SMTP via MailKit) notification channels, both verified end-to-end locally
(WireMock.Net for Slack, MailHog for Email). The Admin API (Phase 08) composes all of this via DI:
minimal-API CRUD for AlertRules/Channels/Subscriptions, read-only Notification history, a manual
trigger-simulated-event endpoint, API-key auth (`IEndpointFilter`), `ProblemDetails` error mapping
(`IExceptionHandler`), and OpenAPI generation. Phase 09 automates the pipeline: an
`EventIngestionWorker` hosted service polls on the configured interval so events flow end-to-end
without manual triggering, channel sends go through a Polly retry/backoff pipeline, and
correlation IDs (per ingestion run and per normalized event) are threaded through ingestion,
matching, and dispatch via `ILogger` scopes. Phase 10 adds operational maturity: Serilog
structured logging (console sink, configured from the `Serilog` appsettings section), a
`/health` endpoint backed by a Postgres dependency check, a `UserSecretsId` enabling local
secret storage, and a hardened `NotifyMeExceptionHandler` that maps every otherwise-unhandled
exception to a generic 500 `ProblemDetails` (logged server-side, no internals leaked to
clients). Phase 11 hardens the test suite: the Admin API and repository integration tests now
run against an ephemeral `Testcontainers.PostgreSql` container (no more fixed local Postgres
dependency), the WireMock-based Slack suite gained webhook-failure and webhook-unreachable
cases, and a coverage review closed real gaps (Slack's network-exception handling,
`NotifyMeExceptionHandler`'s catch-all 500 path) while confirming the matching logic
(`AlertMatcher`) and dispatch retry path (`DispatchNotificationUseCase`) are already fully
covered - 110 tests passing total. Phase 12 adds `.github/workflows/ci.yml` (restore/build/test
on push and pull request to `main`, with a MailHog service container for the email check), a
rewritten root `README.md` with concrete quick-start instructions replacing the stale
scaffolding-only description, and a completeness review of `ai/decisions/adr/` (no gaps found).
With the user's explicit go-ahead, an annotated `v1.0.0` git tag was created locally on `main`
marking the backend + frontend as feature-complete for this exercise (not pushed).

The frontend (deferred since the plan's start until the Admin API contract was stable) is now
planned: ADR-0009 decides the stack (Vite + React + TypeScript, OpenAPI-generated types,
TanStack Query, React Router, deliberately no CSS/UI framework or form library, API key in
`sessionStorage`), and Phases 13-15 break the work into foundation/API integration, the actual
CRUD/read screens, and testing/CI/docs, respectively. Phase 13 is done: `frontend/` is scaffolded
and end-to-end verified against the real API (API key entry, typed `openapi-fetch` client
generated from the Admin API's own OpenAPI document, routing shell, `/health` status page); this
also surfaced and fixed a real gap in Phase 08's OpenAPI output (endpoints had no typed response
schemas) via `.Produces<T>()` metadata, and required a new CORS policy on `NotifyMe.Api`. Phase 14
is done too: Alert Rules/Channels/Subscriptions/Notifications screens with create/edit/delete
where the API supports it, a manual trigger-simulated-event action, and TanStack Query cache
invalidation - verified end-to-end through the actual UI, including cleaning the test data back
out. This also surfaced a real Admin API gap: neither `UpdateAlertRuleRequest` nor
`UpdateChannelConfigRequest` support changing `isEnabled` after creation, so the edit forms only
expose that field at creation time. Phase 15 is done: a Vitest + React Testing Library suite
covers the API client (`X-Api-Key` header attachment, 401 handling), `ProblemDetails`/`ApiError`
parsing, and the Alert Rules screen's query/filter/create wiring (mocking `useAdminApiClient`, no
real backend needed); `.github/workflows/ci.yml` gained a second `frontend` job
(`npm ci`/`npm run build`/`npm test`) alongside the existing backend job; and the root `README.md`,
`docs/runbook.md`, `docs/architecture/overview.md`, and `frontend/README.md` were updated to
describe the frontend as done rather than deferred. This also surfaced a Windows-specific Vitest
quirk (worker-pool crash when invoked via `npm --prefix <dir> run test` instead of a plain `cd`
first), documented in `frontend/README.md` and `docs/runbook.md` so CI/local runs avoid it.
