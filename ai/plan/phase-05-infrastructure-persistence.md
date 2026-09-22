# Phase 05 - Infrastructure: persistence

## Goal
Persist domain data in PostgreSQL via EF Core, implementing the repository interfaces defined
in Phase 04.

## Depends on
Phase 04 (repository interfaces must exist).

## Steps
- [x] Add `NotifyMeDbContext` with entity type configurations for all domain entities.
- [x] Add initial EF Core migration.
- [x] Wire Npgsql provider and connection string via configuration.
- [x] Implement `IAlertRuleRepository`, `ISubscriptionRepository`, `IChannelConfigRepository`,
      `INotificationRepository`.
- [x] Add a small integration test that round-trips an `AlertRule` through the repository
      against the docker-compose Postgres instance.

## Verification
- `dotnet ef database update` succeeds against the local docker-compose Postgres.
- Round-trip integration test passes.

## Design notes / course-corrections
- `MatchCriteria` (a Domain value object nested in `AlertRule`) is mapped as an EF Core owned
  entity (`OwnsOne`) rather than a separate table. Its `Keywords` collection
  (`IReadOnlyCollection<string>`) is stored as a single JSON `text` column via a custom
  `ValueConverter`/`ValueComparer` pair, since EF Core cannot natively map raw primitive
  collections as a column.
- Enum properties (`EventCategory`, `Severity`/`MinimumSeverity`, `NotificationStatus`) are
  mapped with `HasConversion<string>()` for human-readable database values instead of raw ints.
- `RawEvent`/`NormalizedEvent` intentionally have no `DbSet`/table: per the Phase 04 design
  they are transient, never persisted.
- Repository read methods that return a single tracked aggregate for later mutation
  (`GetByIdAsync`) leave EF change tracking on; list/query methods use `.AsNoTracking()`.
  Each mutating method (`AddAsync`/`UpdateAsync`/`DeleteAsync`) calls `SaveChangesAsync`
  directly; no separate Unit-of-Work abstraction was introduced (kept minimal, no current need
  for cross-repository transactions).
- A design-time `IDesignTimeDbContextFactory<NotifyMeDbContext>` was added so `dotnet ef`
  commands work without the Api's DI composition root (composition is deferred to Phase 08).
  It reads the connection string from `NOTIFYME_CONNECTION_STRING`, falling back to the
  docker-compose defaults.
- `AddNotifyMePersistence(IServiceCollection, string)` registers the `DbContext` (Npgsql) and
  all four repository interfaces as Scoped; intended to be called from Phase 08's Api
  composition root.
- The round-trip integration test lives in `tests/NotifyMe.IntegrationTests/Persistence/` and
  runs against the docker-compose Postgres directly (requires `docker compose up -d postgres`
  first), per this phase's own wording. A fully self-contained Testcontainers-based equivalent
  is planned for Phase 11 alongside the rest of the API integration suite.
- Pitfall avoided: an owned entity instance (`MatchCriteria`) cannot be shared across two
  different owner (`AlertRule`) instances in the same `DbContext`; EF Core throws because the
  owned entity's shadow key is derived from its owner. Each test fixture `AlertRule` needs its
  own `MatchCriteria` instance.
- Pinned `Microsoft.EntityFrameworkCore.Relational` to 10.0.12 explicitly in
  `NotifyMe.Infrastructure.csproj` to resolve an MSB3277 version-conflict warning caused by
  `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3's own minimum dependency on an older
  `EntityFrameworkCore.Relational` (10.0.4).

## Status
Done (2026-09-22).

