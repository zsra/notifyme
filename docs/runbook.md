# Runbook

## Local setup

1. `docker compose up -d` - starts PostgreSQL and MailHog.
2. `dotnet user-secrets set "Admin:ApiKey" "<your local key>" --project src/NotifyMe.Api` (and
   any other secrets, e.g. a real Slack webhook URL as a channel's `Target` via the Admin API
   itself, not via configuration). Configuration is layered
   `appsettings.json` -> `appsettings.{Environment}.json` -> user secrets (Development only) ->
   environment variables, per the standard ASP.NET Core host configuration order; in CI/deployment,
   set `Admin__ApiKey` and `ConnectionStrings__Postgres` (or `NOTIFYME_CONNECTION_STRING`) as
   environment variables instead of user-secrets.
3. `dotnet user-secrets set "Jwt:SigningKey" "<a long random string, 32+ bytes>" --project src/NotifyMe.Api`
   - signs/validates end-user JWT bearer tokens for the self-service `/api/me/*` endpoints (see
   [ADR-0010](../ai/decisions/adr/0010-end-user-self-service.md)); set `Jwt__SigningKey` as an
   environment variable in CI/deployment instead. Entirely separate from `Admin:ApiKey` - the two
   never grant access to each other's endpoints.
4. `dotnet ef database update --project src/NotifyMe.Infrastructure --startup-project src/NotifyMe.Api`
   - applies migrations.
5. `dotnet run --project src/NotifyMe.Api` - starts the API.
6. Open the OpenAPI document at the API's `/openapi/v1.json` endpoint (development only) to
   explore the Admin API.
7. Open MailHog's web UI (default `http://localhost:8025`) to see delivered emails.
8. `GET /health` (no API key required) reports `Healthy`/`Unhealthy` based on Postgres
   reachability.
9. Use `POST /api/admin/events/trigger-simulated` (with the `X-Api-Key` header) to force an event
   through the pipeline on demand instead of waiting for `EventIngestionWorker`'s polling interval.
10. Register an end-user account with `POST /api/auth/register` (`{ "email", "password" }`, no
    auth header required) to get a JWT bearer token, then call `/api/me/alert-rules`,
    `/api/me/channels`, `/api/me/subscriptions` with `Authorization: Bearer <token>` to manage
    that user's own rows (see ADR-0010).

## Logging

Structured logging is provided by Serilog (`Serilog.AspNetCore`), configured entirely from the
`Serilog` appsettings section (console sink by default). Each ingestion run and each normalized
event carries a correlation id (`IngestionRunId`/`CorrelationId` log-scope properties) threaded
through `IngestEventsUseCase` and `DispatchNotificationUseCase`, so a single event's journey from
fetch through dispatch can be traced in the console output.

## Running tests

- `dotnet test` at the repo root runs unit tests.
- Integration tests require a reachable Docker daemon. Postgres-backed tests
  (`AdminApiWebApplicationFactory`-based Api tests and `AlertRuleRepositoryTests`) spin up their
  own ephemeral `postgres:16-alpine` container via Testcontainers - no manual setup needed beyond
  Docker being available. MailHog-backed email tests still require
  `docker compose up -d mailhog` (a real SMTP+web-UI pair isn't worth spinning up per test run).
  WireMock.Net-backed Slack tests spin up their own in-process server, no Docker needed at all.
- `dotnet test --collect:"XPlat Code Coverage"` produces per-project Cobertura XML under a
  results directory; merge/view with `reportgenerator` (`dotnet tool install -g
  dotnet-reportgenerator-globaltool`, then `reportgenerator -reports:<dir>\**\coverage.cobertura.xml
  -targetdir:coverage-report -reporttypes:Html`).

## Frontend

A React/TypeScript frontend lives in `frontend/` (see ADR-0009 and
`ai/plan/phase-13-frontend-foundation.md`), hosting both the Admin panel and the end-user
self-service screens (Phase 16, see ADR-0010). With the API running per "Local setup" above:

```
cd frontend
npm install
npm run dev
```

Open `http://localhost:5173`. For the Admin panel (`/`), enter the `Admin:ApiKey` value
configured on the backend. For self-service (`/register` or `/login`), create/use an end-user
account instead - no Admin API key involved. The API's `Cors:FrontendOrigin` setting
(`appsettings.json`, defaults to `http://localhost:5173`) must match wherever the frontend dev
server actually runs.

Run its test suite with `cd frontend && npm test` (Vitest + React Testing Library; see
`frontend/README.md#testing`). Run it from inside `frontend/`, not via `npm --prefix frontend
run test` from the repo root - `--prefix` triggers a Vitest worker-pool crash on Windows.


