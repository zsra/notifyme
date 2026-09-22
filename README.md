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
  [`docs/architecture/overview.md`](docs/architecture/overview.md)). Scaffolded and building;
  no business logic yet.
- `tests/` - automated tests: `NotifyMe.Domain.Tests`, `NotifyMe.Application.Tests`,
  `NotifyMe.IntegrationTests` (xUnit). Scaffolded; no tests written yet.

## Status

Solution and project scaffolding is in place (`dotnet build` succeeds across all 7 projects);
no domain/application logic has been implemented yet. See
[`ai/plan/00-plan-overview.md`](ai/plan/00-plan-overview.md) for the phased plan and current
progress, and [`ai/decisions/adr/`](ai/decisions/adr/) for the key decisions made so far.

The original brief is preserved at [`task-04-feature-design-and-build.docx`](task-04-feature-design-and-build.docx).

Frontend is intentionally out of scope for now; the backend (.NET, Clean Architecture) is being
built first and the frontend will be scoped in a follow-up once the Admin API contract is stable.
