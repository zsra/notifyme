# AGENTS.md

Guidance for any AI coding agent (or human) working in this repository. Read this before making
changes.

## What this project is

NotifyMe: a backend (.NET, Clean Architecture) that ingests events, matches them against
user-defined alert rules, and dispatches notifications over pluggable channels (Slack, Email,
more later). See [`README.md`](README.md) for the elevator pitch and [`docs/architecture/overview.md`](docs/architecture/overview.md)
for the full picture.

## Where to look before you start

1. [`ai/plan/00-plan-overview.md`](ai/plan/00-plan-overview.md) - current phase, what's done,
   what's next. Always check this first.
2. [`ai/decisions/adr/`](ai/decisions/adr/) - every non-trivial decision already made, with
   context and alternatives considered. Do not silently contradict an accepted ADR; if you think
   one is wrong, add a new ADR that supersedes it and explain why.
3. [`docs/architecture/`](docs/architecture/) and [`docs/api/`](docs/api/) - the actual design
   docs. Code should match these; if they drift apart, fix the doc or flag the mismatch, don't
   just ignore it.

## Repo conventions

- **Layering (Clean Architecture)**: `Domain` has no outward dependencies. `Application` depends
  only on `Domain`. `Infrastructure` implements interfaces defined in `Domain`/`Application`.
  `Api` composes everything via DI. Never reference `Infrastructure` from `Domain` or
  `Application`.
- **The event-ingestion seam**: event sources are simulated for now behind `IEventSource`. Keep
  that boundary intact, see [`docs/architecture/event-ingestion.md`](docs/architecture/event-ingestion.md).
  Don't couple the simulator's internals into `Domain`/`Application`.
- **Channels are pluggable**: adding a new notification channel should mean "add one class +
  one DI registration", not touching core domain types. See
  [`docs/architecture/notification-channels.md`](docs/architecture/notification-channels.md).
- **Secrets**: never commit real webhook URLs, SMTP credentials, or API keys anywhere in this
  repo, including under `ai/` or `docs/`. Use `dotnet user-secrets` locally and environment
  variables in CI. Use obviously-fake placeholder values in docs/examples.
- **Commit messages**: `feat(phase-N): <summary>` for implementation, `docs(phase-N): <summary>`
  for documentation-only changes, referencing the plan phase number from
  `ai/plan/00-plan-overview.md`.
- **Decision logging**: if you make a non-trivial design choice that isn't already covered by an
  ADR, add a new file under `ai/decisions/adr/` using `ai/decisions/adr/template.md`.
- **No large monolithic files**: split plan/decision/design content into small, topic-scoped
  files rather than one large document.

## Testing expectations

Domain and Application logic get unit tests (xUnit). Cross-layer behavior (API + DB + channels)
gets integration tests (WebApplicationFactory + Testcontainers). Don't mark a phase complete
without `dotnet build` and `dotnet test` passing.
