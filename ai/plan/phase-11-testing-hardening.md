# Phase 11 - Testing hardening

## Goal
Round out the test suite so the full pipeline is verified automatically, not just by manual
checks.

## Depends on
Phase 09 (pipeline must be functionally complete).

## Steps
- [x] Integration tests using `WebApplicationFactory` + `Testcontainers.PostgreSql` for the
      Admin API against a real (ephemeral) database.
- [x] WireMock.Net-based integration tests confirming Slack webhook payloads.
- [x] MailHog-based (or equivalent) integration tests confirming email delivery and content.
- [x] Generate and review a coverage report; look for untested branches in the matching logic
      and channel dispatch retry paths specifically.

## Verification
- Full `dotnet test` suite passes locally in a clean environment (fresh containers).
- Coverage report reviewed; gaps in critical paths (matching, dispatch, auth) are either closed
  or explicitly noted as accepted risk.

## Design notes / course-corrections

- **Testcontainers migration**: added `Testcontainers.PostgreSql`, and a new
  `PostgresContainerFixture` (`tests/NotifyMe.IntegrationTests/PostgresContainerFixture.cs`)
  that starts one ephemeral `postgres:16-alpine` container and runs EF Core migrations against
  it once. Shared across every test class via an xUnit `[CollectionDefinition("Postgres")]` +
  `ICollectionFixture<PostgresContainerFixture>` (one container for the whole run, not one per
  test class - starting 7+ separate containers would be slow and wasteful). `AdminApiWebApplicationFactory`
  and `AlertRuleRepositoryTests` now take `PostgresContainerFixture` as a constructor parameter
  (resolved by xUnit through the shared collection) instead of a hardcoded
  `Host=localhost;Port=5432;...` connection string, removing the previous requirement to have
  `docker compose up -d postgres` running with that exact fixed configuration before `dotnet test`
  would pass.
  - Course-correction: `PostgreSqlBuilder`'s parameterless constructor is obsolete in the
    installed `Testcontainers.PostgreSql` version; used `new PostgreSqlBuilder("postgres:16-alpine")`
    instead of `new PostgreSqlBuilder().WithImage(...)`.
- **WireMock/MailHog tests**: both already existed from Phase 07
  (`SlackNotificationChannelWireMockTests`, `EmailNotificationChannelMailHogTests`). Extended the
  Slack suite with two new cases this phase: a webhook-returns-404 case (asserts the failure
  message includes the status code and body) and a webhook-unreachable case (stops the WireMock
  server, forcing an `HttpRequestException`) - the latter closes a real coverage gap: the
  `catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)` branch in
  `SlackNotificationChannel` had 0% line coverage before this, meaning nothing verified that a
  network failure actually gets converted into a `Failure` result rather than propagating as an
  unhandled exception (which matters because `DispatchNotificationUseCase`'s Polly retry pipeline
  only retries on a `Failure` *result*, not on a thrown exception).
- **Coverage review** (`coverlet.collector` + `dotnet-reportgenerator-globaltool`, merged via
  `reportgenerator -reports:coverage-results\**\coverage.cobertura.xml`): overall line coverage
  ~77.5%, `NotifyMe.Application` 100%, `NotifyMe.Infrastructure` ~97%, `NotifyMe.Domain` ~91%.
  Specifically checked the two areas called out in this phase's goal:
  - **Matching logic** (`AlertMatcher`): 100% line and branch coverage already, no gap.
  - **Channel dispatch retry paths** (`DispatchNotificationUseCase`): 100% line coverage,
    ~93% branch coverage. The one uncovered branch is
    `notification.MarkFailed(result.ErrorMessage ?? "Unknown error.")`'s null-coalescing fallback;
    `NotificationDispatchResult.Failure(string errorMessage)` only accepts a non-null message, so
    this fallback is unreachable through the current public API and is accepted as a harmless
    defensive guard rather than a real gap.
  - Also found and closed: `SlackNotificationChannel`'s exception-handling branch (0% -> covered,
    see above) and `NotifyMeExceptionHandler`'s catch-all/unexpected-exception branch (previously
    untested because no endpoint in the suite happened to throw an unrecognized exception type) -
    added a new `NotifyMeExceptionHandlerTests.cs` that calls `TryHandleAsync` directly with a
    `DefaultHttpContext` for all three of `NotFoundException`/`ValidationException`/the catch-all
    case, rather than relying on an HTTP round trip.
  - Remaining lower-coverage areas (`Microsoft.AspNetCore.OpenApi.Generated` at 1%, various
    Domain value-object defensive guard clauses) are accepted as low-value/auto-generated code,
    not treated as gaps.
- Full solution `dotnet test`: **110/110 passing** (105 at the end of Phase 10, plus 2 new Slack
  webhook-failure tests and 3 new `NotifyMeExceptionHandlerTests` added this phase).

### Post-Phase 12 bug fix: flaky connection string in `AdminApiWebApplicationFactory`
A real CI run (after the Phase 12 NuGet Audit remediation) intermittently failed 6 of the
`WebApplicationFactory`-based API tests with `Npgsql.NpgsqlException: Failed to connect to
127.0.0.1:5432` / connection refused, even though the shared `PostgresContainerFixture`'s
container reported itself ready moments earlier. First (incomplete) fix attempt: hardened
`AdminApiWebApplicationFactory` to read `PostgresContainerFixture.ConnectionString` lazily
inside `ConfigureAppConfiguration` instead of capturing it eagerly in its constructor - a real
latent bug, but not the actual cause of the CI failures, since the very next CI run failed
identically even with that fix in place.

**Actual root cause**, found by reproducing the failure locally (had to `docker stop` the local
docker-compose Postgres container first - it had been running on port 5432 for hours and was
silently masking the bug the whole time, which is exactly why every local `dotnet test` run had
been passing): `src/NotifyMe.Api/Program.cs` resolved the Postgres connection string *eagerly*,
as a plain local variable, in the top-level statements between `WebApplication.CreateBuilder(args)`
and `builder.Build()`. `WebApplicationFactory<Program>`'s `ConfigureWebHost` -> `ConfigureAppConfiguration`
override (how `AdminApiWebApplicationFactory` points the app at its ephemeral Testcontainers
Postgres) is only merged into the final `IConfiguration` during `builder.Build()` - it is never
visible to code that reads `builder.Configuration` directly *before* that call. `Admin:ApiKey`
and `EventIngestion:Simulated:PollingInterval` were unaffected because those are consumed later,
via DI-bound options resolved after `Build()`; only the connection string was read too early, so
it always fell through to the hardcoded `Host=localhost;Port=5432;...` default - deterministically,
every time, in CI, where nothing is listening on that port.

Fixed by moving connection-string resolution out of `Program.cs` entirely: `AddNotifyMePersistence`
now takes `IConfiguration` (instead of a pre-resolved `string`) and resolves the connection string
lazily inside the `AddDbContext` options delegate, which EF Core invokes at DI-resolution time -
always after `Build()` has run and any `WebApplicationFactory` configuration overrides have been
merged in. See `src/NotifyMe.Api/Program.cs` and
`src/NotifyMe.Infrastructure/Persistence/PersistenceServiceCollectionExtensions.cs`. Verified by
stopping the local docker-compose Postgres (removing the accidental local masking) and running the
full `dotnet test` (110/110 passing) with nothing else listening on port 5432 - a genuine
CI-equivalent local repro and fix, not just a hopeful local pass.

## Status
Done (2026-09-22).

