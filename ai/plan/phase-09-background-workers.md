# Phase 09 - Background workers

## Goal
Automate the pipeline so simulated events flow through ingestion, matching, and dispatch without
manual triggering.

## Depends on
Phase 08 (API/DI composition root must exist to host the workers).

## Steps
- [ ] `EventIngestionWorker` (HostedService): polls `IEventSource` on a configurable interval.
- [ ] `AlertEvaluationAndDispatchWorker`: evaluates normalized events against alert rules and
      dispatches via the resolved channel(s).
- [ ] Polly-based retry/backoff around channel sends.
- [ ] Correlation IDs threaded through ingestion -> match -> dispatch for traceability.

## Verification
- Running the API locally (with docker-compose up) results in simulated events flowing
  end-to-end into MailHog/WireMock without any manual trigger call.
- A transient channel failure is retried per the configured policy rather than silently dropped.

## Status
Not started.
