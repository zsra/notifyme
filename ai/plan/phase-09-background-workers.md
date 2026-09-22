# Phase 09 - Background workers

## Goal
Automate the pipeline so simulated events flow through ingestion, matching, and dispatch without
manual triggering.

## Depends on
Phase 08 (API/DI composition root must exist to host the workers).

## Steps
- [x] `EventIngestionWorker` (HostedService): polls `IEventSource` on a configurable interval.
- [x] `AlertEvaluationAndDispatchWorker`: evaluates normalized events against alert rules and
      dispatches via the resolved channel(s).
- [x] Polly-based retry/backoff around channel sends.
- [x] Correlation IDs threaded through ingestion -> match -> dispatch for traceability.

## Verification
- Running the API locally (with docker-compose up) results in simulated events flowing
  end-to-end into MailHog/WireMock without any manual trigger call.
- A transient channel failure is retried per the configured policy rather than silently dropped.

## Design notes / course-corrections
- No separate `AlertEvaluationAndDispatchWorker` was added. `IngestEventsUseCase` (Application,
  since Phase 04/06) already fuses fetch -> normalize -> evaluate -> dispatch into one atomic
  call; splitting evaluation/dispatch into a second hosted service would just mean two workers
  coordinating around one already-atomic use case, for no benefit. This mirrors the Phase 07
  course-correction of not adding a standalone `ChannelResolver` type. `EventIngestionWorker`
  (new, `src/NotifyMe.Api/Workers/EventIngestionWorker.cs`) is a `BackgroundService` that resolves
  `IngestEventsUseCase` from a fresh DI scope (via `IServiceScopeFactory`, since the use case's
  dependencies are `Scoped`) on each tick of a `PeriodicTimer`, runs one pass immediately on
  startup (rather than waiting a full interval first, so a fresh `dotnet run` demoes without a
  manual trigger), and logs+swallows any single-pass exception so one bad tick doesn't kill
  polling. Reuses the existing `EventIngestion:Simulated:PollingInterval` config value (already
  anticipated for this exact purpose in `SimulatedEventSourceOptions`'s doc comment from Phase 06)
  rather than inventing a second, source-agnostic interval setting.
- Polly retry is applied once, generically, around the `INotificationChannel.SendAsync` call
  inside `DispatchNotificationUseCase` (Application layer) rather than duplicated inside each
  channel implementation. A `ResiliencePipeline<NotificationDispatchResult>` (Polly v8) retries
  while `!result.IsSuccess` (channels already convert exceptions into a `Failure` result rather
  than throwing, so retrying on the result value rather than on a caught exception matches the
  existing "every outcome is a terminal state, nothing throws" contract), with exponential
  backoff. Configured via the new `Resilience:ChannelSend` section
  (`ChannelSendResilienceOptions`: `MaxRetryAttempts` default 3, `BaseDelay` default 1s). The
  pipeline is injected as an optional constructor parameter defaulting to
  `ResiliencePipeline<NotificationDispatchResult>.Empty` (no-op) so every pre-existing unit test
  that constructs `DispatchNotificationUseCase` directly keeps its exact original call-count
  behavior without modification; two new tests
  (`ExecuteAsync_TransientFailureThenSuccess_RetriesAndMarksNotificationSent`,
  `ExecuteAsync_FailuresExceedMaxRetryAttempts_MarksNotificationFailedAfterExhaustingRetries`)
  exercise the retry path explicitly with a zero-delay pipeline and a new
  `SequencedNotificationChannel` test double.
- Correlation IDs: each `IngestEventsUseCase.ExecuteAsync` call gets a fresh "ingestion run" id,
  and each `NormalizedEvent` reuses its own `Id` (the same value already persisted as
  `Notification.NormalizedEventId`) as its correlation id, applied via
  `ILogger.BeginScope(...)` in both `IngestEventsUseCase` and `DispatchNotificationUseCase`. This
  is deliberately built on plain `Microsoft.Extensions.Logging` (`ILogger<T>`, new
  `Microsoft.Extensions.Logging.Abstractions` package reference in `NotifyMe.Application`), not
  Serilog: Phase 10 is where "Serilog structured logging across ingestion/match/dispatch" is
  explicitly planned, and `BeginScope` scopes flow through to Serilog (or any other
  `ILogger`-compatible sink) unchanged once that's wired up, so this phase's correlation-id work
  isn't wasted, just not yet exported as structured/queryable log fields.
- `ApplicationServiceCollectionExtensions.AddNotifyMeApplication` now takes an `IConfiguration`
  parameter (to bind `ChannelSendResilienceOptions`), mirroring the Infrastructure layer's
  `AddSimulatedEventSource(configuration)` / `AddNotifyMeNotificationChannels(configuration)`
  pattern; `Program.cs` updated accordingly. New package references needed to support this:
  `Microsoft.Extensions.Logging.Abstractions`, `Microsoft.Extensions.Configuration.Abstractions`,
  `Microsoft.Extensions.Options`, `Microsoft.Extensions.Options.ConfigurationExtensions` (all
  10.0.12, matching the other layers' package versions), and `Polly` (8.8.0).
- `AdminApiWebApplicationFactory` (integration tests) now overrides
  `EventIngestion:Simulated:PollingInterval` to 1 day, since the real `EventIngestionWorker` now
  runs automatically inside the test host; without this override it could fire an uncontrolled
  extra ingestion pass mid-test-run. Every integration test already triggers ingestion explicitly
  via the trigger-simulated endpoint, so this only removes a source of potential flakiness with
  no loss of coverage.
- Full-solution `dotnet test` after all Phase 09 work: 104/104 tests passing (102 from Phase 08
  plus 2 new retry tests).

## Status
Done (2026-09-22).
