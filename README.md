# NotifyMe

An alerting platform that lets users configure alerts for important world events (breaking
news, market movements, natural disasters) and get notified via Email and Slack, with more
channels addable later. Built from a deliberately vague product brief as part of an exercise in
AI-directed software design and delivery.

## Where things live

- [`ai/`](ai/) - process artifacts: the implementation plan and the decision log (ADRs). Read
  `ai/README.md` first.
- [`docs/`](docs/) - actual design documentation: architecture, data model, API contracts, and
  the runbook for running the system locally.
- `src/` - the .NET solution (`NotifyMe.slnx`): `NotifyMe.Domain`, `NotifyMe.Application`,
  `NotifyMe.Infrastructure`, `NotifyMe.Api`, wired per Clean Architecture (see
  [`docs/architecture/overview.md`](docs/architecture/overview.md)).
- `tests/` - automated tests: `NotifyMe.Domain.Tests`, `NotifyMe.Application.Tests`,
  `NotifyMe.Infrastructure.Tests`, `NotifyMe.IntegrationTests` (xUnit).

## Quick start

Prerequisites: [.NET 10 SDK](https://dotnet.microsoft.com/download), Docker (for PostgreSQL,
MailHog, and Testcontainers-backed integration tests).

```
docker compose up -d
dotnet user-secrets set "Admin:ApiKey" "<your local key>" --project src/NotifyMe.Api
dotnet ef database update --project src/NotifyMe.Infrastructure --startup-project src/NotifyMe.Api
dotnet run --project src/NotifyMe.Api
```

Then open the OpenAPI document at `/openapi/v1.json` (development only) to explore the Admin
API, or `GET /health` for a liveness/Postgres-connectivity check. See
[`docs/runbook.md`](docs/runbook.md) for the full walkthrough, including how to configure a real
Slack webhook channel and inspect delivered emails via MailHog's web UI.

Run the test suite with `dotnet test` from the repo root; see
[`docs/runbook.md`](docs/runbook.md#running-tests) for what each test project requires (Docker
for Testcontainers-backed Postgres tests, `docker compose up -d mailhog` for the MailHog email
check, nothing extra for WireMock-backed Slack tests).

## Status

The backend is feature-complete for this exercise: Domain and Application layers implement
alert matching and notification dispatch; Infrastructure provides real EF Core/PostgreSQL
persistence, a deterministic simulated event source, and real Slack (webhook) and Email (SMTP)
notification channels; the Admin API (Phase 08) exposes CRUD for alert rules/channels/
subscriptions, notification history, and a manual trigger endpoint behind API-key auth; a
background worker (Phase 09) automates event ingestion end-to-end; Serilog logging, a `/health`
endpoint, and hardened error handling round out the cross-cutting concerns (Phase 10); and the
test suite (Phase 11) runs against ephemeral Testcontainers-backed Postgres, WireMock-backed
Slack, and MailHog-backed Email, with 110+ tests passing and CI (`.github/workflows/ci.yml`)
running restore/build/test on every push and pull request. See
[`ai/plan/00-plan-overview.md`](ai/plan/00-plan-overview.md) for the phased plan and detailed
status, and [`ai/decisions/adr/`](ai/decisions/adr/) for the key decisions made along the way.

The original brief is preserved at [`task-04-feature-design-and-build.docx`](task-04-feature-design-and-build.docx).

The frontend (a React/TypeScript admin panel, kept deliberately simple and technical rather than
visual) is now planned but not yet built: see ADR-0009 and Phases 13-15 in
[`ai/plan/00-plan-overview.md`](ai/plan/00-plan-overview.md).
