# Phase 05 - Infrastructure: persistence

## Goal
Persist domain data in PostgreSQL via EF Core, implementing the repository interfaces defined
in Phase 04.

## Depends on
Phase 04 (repository interfaces must exist).

## Steps
- [ ] Add `NotifyMeDbContext` with entity type configurations for all domain entities.
- [ ] Add initial EF Core migration.
- [ ] Wire Npgsql provider and connection string via configuration.
- [ ] Implement `IAlertRuleRepository`, `ISubscriptionRepository`, `IChannelConfigRepository`,
      `INotificationRepository`.
- [ ] Add a small integration test that round-trips an `AlertRule` through the repository
      against the docker-compose Postgres instance.

## Verification
- `dotnet ef database update` succeeds against the local docker-compose Postgres.
- Round-trip integration test passes.

## Status
Not started.
