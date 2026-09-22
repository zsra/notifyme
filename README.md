# NotifyMe

An alerting platform that lets users configure alerts for important world events (breaking
news, market movements, natural disasters) and get notified via Email and Slack, with more
channels addable later. Built from a deliberately vague product brief as part of an exercise in
AI-directed software design and delivery.

## Where things live

- [`ai/`](ai/) - process artifacts: the implementation plan, the decision log (ADRs), and the
  prompt history used while directing AI throughout this project. Read `ai/README.md` first.
- [`docs/`](docs/) - actual design documentation: architecture, data model, API contracts, and
  the runbook for running the system locally.
- `src/` - the .NET solution (not created yet, see plan status below).
- `tests/` - automated tests (not created yet, see plan status below).

## Status

Currently in the design/planning phase. No application code has been written yet. See
[`ai/plan/00-plan-overview.md`](ai/plan/00-plan-overview.md) for the phased plan and current
progress, and [`ai/decisions/adr/`](ai/decisions/adr/) for the key decisions made so far.

The original brief is preserved at [`task-04-feature-design-and-build.docx`](task-04-feature-design-and-build.docx).

Frontend is intentionally out of scope for now; the backend (.NET, Clean Architecture) is being
built first and the frontend will be scoped in a follow-up once the Admin API contract is stable.
