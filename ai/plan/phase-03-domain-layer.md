# Phase 03 - Domain layer

## Goal
Model the core business concepts with no framework/infrastructure dependencies: entities, value
objects, enums, and the key abstraction interfaces that the rest of the system depends on.

## Depends on
Phase 02 (project must exist to put code in).

## Steps
- [ ] Entities: `AlertRule`, `RawEvent`, `NormalizedEvent`, `Subscription`, `ChannelConfig`,
      `Notification`.
- [ ] Enums/value objects: `EventCategory` (BreakingNews / MarketMovement / NaturalDisaster),
      `NotificationStatus` (Pending / Sent / Failed).
- [ ] Domain interfaces: `IEventSource`, `INotificationChannel`, `IAlertMatcher`.
- [ ] Domain invariants/validation on entities (e.g. an `AlertRule` must reference a valid
      `EventCategory` and have at least one match criterion).
- [ ] Unit tests in `NotifyMe.Domain.Tests` covering entity invariants.

## Verification
- `NotifyMe.Domain` has zero package references to EF Core, ASP.NET Core, HTTP clients, etc.
- `dotnet test tests/NotifyMe.Domain.Tests` passes.

## Status
Not started.
