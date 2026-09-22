# Phase 10 - Cross-cutting concerns

## Goal
Add the operational maturity expected of a real service: structured logging, layered
configuration, health checks, consistent error handling, and safe secrets handling.

## Depends on
Phase 09 (pipeline should be functionally complete before hardening it operationally).

## Steps
- [x] Serilog structured logging across ingestion/match/dispatch.
- [x] Configuration layering: `appsettings.json` -> `appsettings.{Environment}.json` -> user
      secrets (local) -> environment variables (CI/deployment).
- [x] `/health` endpoint with a Postgres dependency check.
- [x] Global exception handling producing consistent `ProblemDetails` responses.
- [x] Confirm no secrets (webhook URLs, SMTP credentials, API key) are committed anywhere;
      real values only ever come from user-secrets or environment variables.

## Verification
- Logs show structured entries for each pipeline stage with correlation IDs.
- `/health` returns healthy when Postgres is up, unhealthy when it's down.
- A repo-wide search confirms no real secret values are committed.

## Design notes / course-corrections

- **Serilog wiring**: `builder.Host.UseSerilog((context, services, configuration) =>
  configuration.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services)
  .Enrich.FromLogContext())` reads everything from the `Serilog` appsettings section (console
  sink, `MinimumLevel.Default = Information`, `Microsoft.AspNetCore` and
  `Microsoft.EntityFrameworkCore.Database.Command` overridden to `Warning` to cut SQL-command
  noise). The old plain `Logging` section was removed from `appsettings.json`/
  `appsettings.Development.json` in favor of the `Serilog` section (Serilog's
  `ReadFrom.Configuration` doesn't read `Logging`).
  - **Course-correction**: the first attempt used the commonly-recommended two-phase pattern
    (`Log.Logger = new LoggerConfiguration()....CreateBootstrapLogger();` plus a
    `try/catch(ex is not HostAbortedException)/finally { Log.CloseAndFlush(); }` wrapper around
    the whole top-level-statement body). That pattern is fine for a normal running process, but it
    broke `NotifyMe.IntegrationTests`: `WebApplicationFactory<Program>` builds this same entry
    point's host more than once within a single test process, and Serilog's bootstrap logger
    freezes itself the first time a host built from it is disposed, so the second host build threw
    `InvalidOperationException: The logger is already frozen.` (5 of 105 tests failed with this).
    Fixed by dropping the static `Log.Logger`/bootstrap-logger/try-catch-finally dance entirely and
    just calling `UseSerilog` directly on `builder.Host` - every host build gets its own logger
    instance, so there's nothing to freeze across builds. Recorded in
    `/memories/repo/dotnet-test-verification.md` as a reusable lesson.
  - Also during this rewrite, a `replace_string_in_file` call that spanned from the `using`
    statements through `app.Run();` accidentally deleted the trailing
    `public partial class Program { }` declaration (needed for
    `WebApplicationFactory<Program>` in the integration tests project) - caught and re-added
    immediately, before any build was attempted.
- **Health check**: `builder.Services.AddHealthChecks().AddDbContextCheck<NotifyMeDbContext>
  ("postgres")` mapped at `app.MapHealthChecks("/health")`, deliberately outside the
  `/api/admin` route group (no `X-Api-Key` required - it's an operational probe, not an admin
  action). Default plain-text `Healthy`/`Unhealthy` response is sufficient; no custom JSON
  formatting added. Covered by a new
  `tests/NotifyMe.IntegrationTests/Api/HealthEndpointTests.cs`.
- **Configuration layering**: no new code needed beyond `UserSecretsId` in
  `NotifyMe.Api.csproj` (`notifyme-api-a1e6f8b2-9c3d-4f7a-8e1b-6d2c5a9f0e3b`) - `WebApplication
  .CreateBuilder` already auto-loads user secrets in Development when that property is present,
  and environment variables already override configuration by default host-builder ordering.
  `docs/runbook.md` rewritten to describe the real layering and the actual config keys in use
  (`Admin:ApiKey`, `ConnectionStrings:Postgres`) instead of the Phase 01 placeholder text.
- **Exception handler**: `NotifyMeExceptionHandler` extended with a catch-all `_ =>` branch
  mapping any exception not already recognized (`NotFoundException`, `ValidationException`,
  `ArgumentException`) to a generic 500 `ProblemDetails` with a fixed, non-descriptive `detail`
  (no exception internals leaked to the client - OWASP: don't expose stack
  traces/internals to end users). The full exception is still logged server-side via an injected
  `ILogger<NotifyMeExceptionHandler>` (`Error` level for 5xx, `Warning` for 4xx), tagged with
  `httpContext.TraceIdentifier`, which is also echoed back in the response's `traceId` extension
  for correlating a client-visible failure with server logs.
- **Secrets scan**: repo-wide regex search for common secret shapes (Slack webhook tokens, AWS
  access key IDs, SendGrid keys, private key headers) found only placeholder values already
  flagged as fake (`https://hooks.slack.com/services/PLACEHOLDER`, `local-dev-admin-key-not-a-
  secret`, `notifyme_dev_only`). No real secrets committed anywhere in the repo.
- Full solution `dotnet test`: **105/105 passing** (104 from Phase 09 plus the new
  `HealthEndpointTests`).

## Status
Done (2026-09-22).
