# Phase 03 - Domain layer

## Goal
Model the core business concepts with no framework/infrastructure dependencies: entities, value
objects, enums, and the key abstraction interfaces that the rest of the system depends on.

## Depends on
Phase 02 (project must exist to put code in).

## Steps
- [x] Entities: `AlertRule`, `RawEvent`, `NormalizedEvent`, `Subscription`, `ChannelConfig`,
      `Notification`.
- [x] Enums/value objects: `EventCategory` (BreakingNews / MarketMovement / NaturalDisaster),
      `NotificationStatus` (Pending / Sent / Failed), `Severity` (Low/Medium/High/Critical), and
      a `MatchCriteria` value object (keywords + minimum severity) factored out of `AlertRule`.
- [x] Domain interfaces: `IEventSource`, `INotificationChannel`, `IAlertMatcher`, plus the
      supporting `NotificationDispatchContext`/`NotificationDispatchResult` records that
      `INotificationChannel` uses.
- [x] Domain invariants/validation on entities, enforced via private constructors + static
      `Create`/factory methods (never public setters): non-empty ids/names, defined enum
      values, and - the interesting one - `MatchCriteria` rejects "no keywords + Severity.Low"
      because that would silently match every event in a category, which is very unlikely to be
      what a user configuring a rule actually wants.
- [x] Unit tests in `NotifyMe.Domain.Tests` covering entity invariants (54 tests: entity
      identity/equality, each entity's `Create` validation, `MatchCriteria` satisfaction logic,
      `AlertRule.Matches`, and the `Notification` Pending -> Sent/Failed state machine).

## Verification
- `NotifyMe.Domain` has zero package references to EF Core, ASP.NET Core, HTTP clients, etc.
  (confirmed: the `.csproj` only sets `TargetFramework`/`Nullable`/`ImplicitUsings`, no
  `PackageReference` entries at all).
- `dotnet test tests/NotifyMe.Domain.Tests` passes: 54 succeeded, 0 failed.
- `dotnet build` for the full solution still succeeds after adding this code.

## Status
Done (2026-09-22). One course-correction during the pass: three of the tests I wrote initially
called `MatchCriteria.Create(keywords: null, Severity.Low)` as a "match anything in this
category" fixture, which the invariant I'd just written correctly rejects. Fixed the test data
(used `Severity.Medium` instead) rather than loosening the domain rule; the failing test run is
the validation working as intended, not a bug in `MatchCriteria`.
