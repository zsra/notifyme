# Phase 04 - Application layer

## Goal
Implement the use cases that orchestrate domain objects, plus the repository interfaces that
Infrastructure will later implement.

## Depends on
Phase 03 (domain entities/interfaces must exist).

## Steps
- [x] Use cases: `CreateAlertRule`, `UpdateAlertRule`, `ManageSubscription`,
      `IngestEventsUseCase`, `EvaluateAlertRulesService`, `DispatchNotificationUseCase`.
- [x] DTOs for use case inputs/outputs (`AlertRuleDto`, `SubscriptionDto`, request records per
      use case, `IngestEventsResult`).
- [x] FluentValidation validators for inputs (`CreateAlertRuleRequestValidator`,
      `UpdateAlertRuleRequestValidator`, `CreateSubscriptionRequestValidator`).
- [x] Repository interfaces: `IAlertRuleRepository`, `ISubscriptionRepository`,
      `IChannelConfigRepository`, `INotificationRepository` (implemented later in Phase 05).
- [x] Unit tests in `NotifyMe.Application.Tests`, especially for `EvaluateAlertRulesService`
      matching logic: true positives, true negatives, disabled-rule exclusion, edge cases
      (empty criteria, multiple matching rules for one event). Also covers `CreateAlertRule`/
      `UpdateAlertRule` (validation failures, not-found), `ManageSubscription` (missing rule/
      channel), `DispatchNotificationUseCase` (send success/failure, disabled channel, unknown
      channel type), and `IngestEventsUseCase` end-to-end wiring - 27 tests total, using hand
      -written in-memory fakes for the repositories instead of a mocking library.

## Design notes / course-corrections
- `AlertRule` (Phase 03) only exposed `Enable()`/`Disable()`; it had no way to change name,
  category, or criteria. Added an `UpdateDetails(name, category, criteria)` method (with the
  same validation as `Create`) since the `UpdateAlertRule` use case needs it. Domain unit tests
  were extended to cover it.
- Raw/normalized events are not persisted in this phase - `IngestEventsUseCase` fetches, then
  normalizes and evaluates entirely in memory, only persisting the resulting `Notification`
  records. Only `IAlertRuleRepository`, `ISubscriptionRepository`, `IChannelConfigRepository`,
  and `INotificationRepository` exist, matching what the phase plan actually asked for; an
  event audit-trail repository can be added later if a real requirement shows up.
- Normalization in `IngestEventsUseCase` is intentionally minimal (payload becomes the title,
  severity defaults to `Medium`) since the real `SimulatedEventSource` payload shape doesn't
  exist yet (Phase 06).

## Verification
- `dotnet test tests/NotifyMe.Application.Tests` passes: 27 succeeded, 0 failed (81 total across
  the solution including Domain.Tests).
- `NotifyMe.Application.csproj` has exactly one package reference (`FluentValidation`) and one
  project reference (`NotifyMe.Domain`) - no EF Core, HTTP, or SMTP libraries.

## Status
Done (2026-09-22). Same lesson repeated from Phase 03: two of my own test fixtures (one in
`EvaluateAlertRulesServiceTests`, three in `AlertRuleTests` back in Phase 03) initially used the
invalid "no keywords + Severity.Low" `MatchCriteria` combination as a lazy "matches everything"
shortcut. The domain's own invariant caught it via a failing test run each time; fixed by using
a real severity threshold instead of loosening the rule.
