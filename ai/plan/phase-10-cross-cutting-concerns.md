# Phase 10 - Cross-cutting concerns

## Goal
Add the operational maturity expected of a real service: structured logging, layered
configuration, health checks, consistent error handling, and safe secrets handling.

## Depends on
Phase 09 (pipeline should be functionally complete before hardening it operationally).

## Steps
- [ ] Serilog structured logging across ingestion/match/dispatch.
- [ ] Configuration layering: `appsettings.json` -> `appsettings.{Environment}.json` -> user
      secrets (local) -> environment variables (CI/deployment).
- [ ] `/health` endpoint with a Postgres dependency check.
- [ ] Global exception handling producing consistent `ProblemDetails` responses.
- [ ] Confirm no secrets (webhook URLs, SMTP credentials, API key) are committed anywhere;
      real values only ever come from user-secrets or environment variables.

## Verification
- Logs show structured entries for each pipeline stage with correlation IDs.
- `/health` returns healthy when Postgres is up, unhealthy when it's down.
- A repo-wide search confirms no real secret values are committed.

## Status
Not started.
