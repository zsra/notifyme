# Phase 04 - Application layer

## Goal
Implement the use cases that orchestrate domain objects, plus the repository interfaces that
Infrastructure will later implement.

## Depends on
Phase 03 (domain entities/interfaces must exist).

## Steps
- [ ] Use cases: `CreateAlertRule`, `UpdateAlertRule`, `ManageSubscription`,
      `IngestEventsUseCase`, `EvaluateAlertRulesService`, `DispatchNotificationUseCase`.
- [ ] DTOs for use case inputs/outputs.
- [ ] FluentValidation validators for inputs.
- [ ] Repository interfaces: `IAlertRuleRepository`, `ISubscriptionRepository`,
      `IChannelConfigRepository`, `INotificationRepository` (implemented later in Phase 05).
- [ ] Unit tests in `NotifyMe.Application.Tests`, especially for `EvaluateAlertRulesService`
      matching logic: true positives, true negatives, disabled-rule exclusion, edge cases
      (empty criteria, multiple matching rules for one event).

## Verification
- `dotnet test tests/NotifyMe.Application.Tests` passes, including negative/edge-case matching
  scenarios, not just the happy path.
- No direct dependency on EF Core, HTTP, or SMTP libraries in this project.

## Status
Not started.
