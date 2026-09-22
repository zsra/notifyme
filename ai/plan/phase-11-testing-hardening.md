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
container reported itself ready moments earlier. Root cause: `AdminApiWebApplicationFactory`'s
constructor captured `postgresFixture.ConnectionString` into a field immediately, but that
property is only populated inside `PostgresContainerFixture.InitializeAsync()` (an
`IAsyncLifetime` callback awaited by xUnit before tests run, not necessarily before every
same-collection fixture is *constructed*). If the class fixture's constructor ran before that
completed, it captured the property's default empty-string value, and an empty Npgsql connection
string silently falls back to Npgsql's own defaults (`localhost`/`5432`) instead of throwing -
which is coincidentally why this passed locally (nothing was listening on `5432` there either
most of the time, but occasionally a stray local Postgres/Testcontainers instance masked it) yet
failed deterministically-ish in CI. Fixed by storing the `PostgresContainerFixture` reference
itself and reading `.ConnectionString` lazily inside the `ConfigureAppConfiguration` callback
(which only runs when the test host is actually built, always after the fixture is fully
initialized), removing the race entirely. See
`tests/NotifyMe.IntegrationTests/Api/AdminApiWebApplicationFactory.cs`. Verified with a full
local `dotnet test` (110/110 passing).

## Status
Done (2026-09-22).
