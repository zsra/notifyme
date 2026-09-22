# Prompt history

An exported, cleaned-up record of the prompts used to direct this project, generated from the
coding tool's own session records at submission time, per the approach decided in
[ADR-0008](../decisions/adr/0008-ai-friendly-scaffolding.md) (see its amendment). This replaces
the hand-maintained prompt log that ADR-0008 explicitly chose not to keep.

Each entry below is a polished, fuller rewrite of what was actually typed (shorthand and typos
cleaned up, order and intent unchanged), followed by a short note on what happened as a result.
A handful of turns were the coding tool's own built-in "continue to iterate?" confirmation rather
than something manually typed; those are labeled as such. All prompts come from a single working
session.

## Kickoff and planning

**Prompt**: Requested a phased implementation plan for the backend first (.NET), with the
frontend to be discussed separately once the backend was settled. Asked for the repository to be
set up in as AI-friendly a way as possible, following current industry best practices, including
a dedicated place to capture the reasoning behind key decisions alongside the implementation plan
itself, broken into small, sequential steps so no meaningful piece of work would be skipped.
Asked for AI-facing process artifacts (plan and decisions) to be kept clearly separate from the
actual design documentation, and for the plan to read the way a lead software engineer would
structure design work and abstractions, treating this as an interactive planning session rather
than a one-shot handoff.

**Outcome**: Established `ai/` (plan + decision log) and `docs/` (design documentation) as
separate folders, added root `AGENTS.md` and `.github/copilot-instructions.md`, and drafted the
16-phase implementation plan (`ai/plan/00-plan-overview.md` plus one file per phase) together
with the first set of ADRs.

---

**Prompt**: Asked to go ahead and commit that planning structure to the repository, with an
explicit instruction to avoid consolidating content into one large file, splitting it into
smaller, topic-scoped files instead, and to hold off on any code implementation for the moment.

**Outcome**: Committed the plan and ADR files as small, topic-scoped documents; no application
code was written yet.

## Backend implementation, phase by phase

**Prompt**: Asked to proceed with implementing the first phase of the plan.

**Outcome**: Implemented Phase 00 (repo and solution scaffolding: `.editorconfig`,
`Directory.Build.props`, `docker-compose.yml`, the empty `NotifyMe.slnx` and project skeletons).

---

**Prompt**: Course-correction: objected to changes being committed automatically, and asked for
the automatic commit to be reverted so changes would remain pending for review before anything
was committed.

**Outcome**: Reverted the automatic commit and left the working tree with pending, uncommitted
changes; committing became an explicit, reviewed step for the rest of the session.

---

**Prompt**: A meta-request about the prompt log itself: asked for the earlier, terse instruction
("Okay let's implement the first part") to be replaced in the log with a more polished,
professionally worded version conveying the same intent.

**Outcome**: Rewrote that entry in the (at the time still hand-maintained) prompt log.

---

**Prompt**: A second course-correction on the same topic: decided to drop the hand-maintained
prompt log entirely, noting that continuously appending to it felt like overhead that would turn
into an ever-growing file over time, and asked for a better long-term approach to capturing
prompt history.

**Outcome**: Deleted the hand-maintained prompt log and recorded the decision as an amendment to
ADR-0008, relying instead on git history plus the coding tool's own session records, exportable
later on demand (this document is that export).

---

**Prompt**: Asked to proceed with the third phase of the plan.

**Outcome**: Implemented Phase 03, the Domain layer (entities, value objects, `IEventSource`/
`INotificationChannel` abstractions), with unit tests.

---

**Prompt**: Asked to proceed with the next phase of the plan.

**Outcome**: Implemented Phase 04, the Application layer (use cases, `AlertMatcher`,
`DispatchNotificationUseCase`), with unit tests.

---

**Prompt**: Asked to proceed with the next phase of the plan.

**Outcome**: Implemented Phase 05, Infrastructure persistence (EF Core/PostgreSQL, repositories).

---

**Prompt**: Asked to proceed with the next step of the current phase.

**Outcome**: Continued and closed out Phase 05's persistence work.

---

**Prompt**: Asked to proceed with the next step of the current phase.

**Outcome**: Implemented Phase 06, the simulated event source (`SimulatedEventSource`,
`IEventSource` seam).

---

**Prompt**: Asked to proceed with the next step of the current phase.

**Outcome**: Implemented Phase 07, the Slack and Email notification channels, verified against
WireMock and MailHog.

---

**Prompt**: The coding tool's own built-in "continue to iterate?" confirmation, accepted as-is to
let the agent keep working through the current phase rather than stopping for a fresh
instruction. (This exact confirmation recurs six times through the session; each occurrence is
listed once, in its actual position, below.)

**Outcome**: Continued the in-progress phase without a new instruction.

---

**Prompt**: Asked to implement the next step of the plan.

**Outcome**: Implemented Phase 08, the Admin API layer (minimal-API CRUD, API-key auth,
`ProblemDetails` error mapping, OpenAPI generation).

---

**Prompt**: The coding tool's own built-in "continue to iterate?" confirmation, accepted as-is.

**Outcome**: Continued the in-progress phase without a new instruction.

---

**Prompt**: Asked to implement the next step of the plan.

**Outcome**: Implemented Phase 09, the background `EventIngestionWorker` and the Polly
retry/backoff pipeline for notification dispatch.

---

**Prompt**: Asked to proceed with the next step of the current phase.

**Outcome**: Implemented Phase 10, cross-cutting concerns (Serilog logging, `/health` endpoint,
`NotifyMeExceptionHandler`, user secrets).

---

**Prompt**: The coding tool's own built-in "continue to iterate?" confirmation, accepted as-is.

**Outcome**: Continued the in-progress phase without a new instruction.

---

**Prompt**: Asked to proceed specifically with phase 12 of the plan (CI and repository polish).

**Outcome**: Added `.github/workflows/ci.yml`, rewrote the root `README.md` with concrete
quick-start instructions, and reviewed the ADR log for completeness. (Phase 11, testing
hardening, was completed via the preceding "continue to iterate" turns.)

## Frontend planning and implementation

**Prompt**: Asked to plan out the frontend using React, explicitly requesting it be kept simple
and focused on technical substance (demonstrating the API integration and workflow cleanly)
rather than on visual polish.

**Outcome**: Wrote ADR-0009 (frontend stack: Vite + React + TypeScript, OpenAPI-generated types,
TanStack Query, React Router, no CSS/UI framework) and drafted Phases 13-15 of the plan.

---

**Prompt**: Asked to start the frontend implementation.

**Outcome**: Implemented Phase 13, the frontend foundation (scaffolding, typed `openapi-fetch`
client generated from the Admin API's own OpenAPI document, API key entry, routing shell,
`/health` status page); this also surfaced and fixed a real gap in Phase 08's OpenAPI output
(missing typed response schemas) and required adding a CORS policy to the API.

---

**Prompt**: The coding tool's own built-in "continue to iterate?" confirmation, accepted as-is.

**Outcome**: Continued the in-progress phase without a new instruction.

---

**Prompt**: Asked to proceed with the next step of the current phase.

**Outcome**: Continued Phase 13/14 work.

---

**Prompt**: The coding tool's own built-in "continue to iterate?" confirmation, accepted as-is.

**Outcome**: Continued the in-progress phase without a new instruction.

---

**Prompt**: Asked to continue with the next phase of the plan.

**Outcome**: Implemented Phase 14, the frontend admin screens (Alert Rules, Channels,
Subscriptions, Notifications, manual event trigger); this surfaced a real Admin API gap
(`isEnabled` not editable after creation on alert rules/channels).

---

**Prompt**: The coding tool's own built-in "continue to iterate?" confirmation, accepted as-is.

**Outcome**: Continued the in-progress phase without a new instruction.

---

**Prompt**: Asked to finish all of the remaining tasks in the plan.

**Outcome**: Implemented Phase 15 (Vitest/React Testing Library suite, frontend CI job, doc
updates), marked the plan as fully complete, and created a local `v1.0.0` git tag.

## Fixing issues found after implementation

**Prompt**: Shared the failing CI run's `dotnet restore` output, showing the build failing on a
high-severity NuGet advisory against `Microsoft.OpenApi` 2.0.0 (GHSA-v5pm-xwqc-g5wc), and asked
for it to be diagnosed and fixed.

**Outcome**: Pinned `Microsoft.OpenApi` to a patched 2.x version in `NotifyMe.Api.csproj` and
confirmed the vulnerability was resolved.

---

**Prompt**: Asked whether there was documentation on how to run the system, and whether there was
a central piece of code that showcased the overall workflow end to end.

**Outcome**: Pointed to the README quick start, `docs/runbook.md`, and the `EventIngestionWorker`/
`DispatchNotificationUseCase` as the central workflow code; no files changed.

---

**Prompt**: Shared a local test run's console output as context for continuing the conversation
about the workflow.

**Outcome**: Used as supporting context while investigating a flaky integration test
(`AdminApiWebApplicationFactory` connection-string capture race).

---

**Prompt**: Pasted a full CI test run as further diagnostic context while tracking down a
flaky/failing integration test.

**Outcome**: Traced the failure past the first (partial) fix to its real root cause, connection
string resolution happening eagerly in `Program.cs` before `WebApplicationFactory`'s
configuration override was merged in, and fixed it by resolving the connection string lazily
inside `AddNotifyMePersistence`. Verified by stopping the local Postgres container and re-running
the full suite (110/110 passing) under CI-equivalent conditions.

---

**Prompt**: Asked how to obtain an admin API key for the system.

**Outcome**: Explained that the key is a self-chosen shared secret set via `dotnet user-secrets`
(or the `Admin__ApiKey` environment variable) and sent via the `X-Api-Key` header; no files
changed.

---

**Prompt**: Pasted the exact error from running `dotnet ef database update`, showing that the
startup project didn't reference `Microsoft.EntityFrameworkCore.Design`, and asked for it to be
resolved.

**Outcome**: Added the `Microsoft.EntityFrameworkCore.Design` package reference to
`NotifyMe.Api.csproj`, fixed an XML-comment syntax error that briefly introduced, and confirmed
`dotnet ef database update` ran successfully.

---

**Prompt**: Asked for frontend launch instructions to be added to the main README as well, so it
would be clear how to start it alongside the backend.

**Outcome**: Added a "Launching the frontend" section to the root `README.md` with the concrete
`npm install`/`npm run dev` steps and login instructions.

## Verification against the original brief

**Prompt**: Asked for the implementation to be verified against the original brief document
(`task-04-feature-design-and-build.docx`), and for any gaps between the brief and the current
implementation to be identified explicitly.

**Outcome**: Extracted the brief's plain text from the `.docx` and produced
`docs/verification/BriefComplianceReport.md`, a point-by-point comparison covering both product
requirements and the brief's process/submission requirements, surfacing several concrete gaps.

## End-user self-service (Phase 16)

**Prompt**: Proposed and confirmed adding end-user self-service on top of the existing
Admin-only system: account registration/login and user-scoped alert rules, channels, and
subscriptions, kept as an additive capability rather than replacing the Admin API.

**Outcome**: Wrote ADR-0010 and `ai/plan/phase-16-user-self-service.md`, then implemented Phase
16 end to end across all four backend layers (a `User` entity, JWT bearer auth separate from the
Admin API key, a nullable `OwnerUserId` on `AlertRule`/`ChannelConfig`/`Subscription` reusing the
existing tables and use cases, new `/api/auth/*` and `/api/me/*` endpoints), with the full
backend test suite passing.

---

**Prompt**: Confirmed proceeding with the frontend half of Phase 16: login/register pages and a
consolidated page for managing one's own alert rules, channels, and subscriptions, plus updating
the docs to describe the new surface.

**Outcome**: Added `/login`, `/register`, and a consolidated `/my` page to the frontend, backed
by a second, independent JWT-based auth flow alongside the existing Admin API key flow; updated
`docs/architecture/overview.md`, `docs/architecture/data-model.md`, `docs/runbook.md`, the root
`README.md`, and `frontend/README.md` to describe the self-service surface; marked Phase 16
complete in the plan overview. Frontend build, lint, and test suite all passing.

---

**Prompt**: Asked for the site's default landing page to change: visiting the app at
`http://localhost:5173/` should land on the login page rather than the Admin panel.

**Outcome**: Moved the Admin panel's routes from `/` to `/admin` (updating its nav links to
match) and made `/` redirect to `/login`, so self-service is now the default landing experience
while the Admin panel remains reachable at `/admin`.
