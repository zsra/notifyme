# Phase 02 - Solution & project scaffolding

## Goal
Create the actual .NET solution structure with the four Clean Architecture projects and three
test projects, wired with the correct dependency direction, plus the local dev infrastructure
(docker-compose for Postgres + MailHog).

## Depends on
Phase 01 (design docs must exist so project boundaries and namespaces are chosen deliberately,
not guessed).

## Steps
- [x] `dotnet new sln` at repo root. Note: the .NET 10 SDK generates the new `NotifyMe.slnx`
      XML solution format by default instead of the legacy `.sln`; kept as-is since it's the
      SDK's current default and both are supported by tooling.
- [x] Create `src/NotifyMe.Domain`, `src/NotifyMe.Application`, `src/NotifyMe.Infrastructure`,
      `src/NotifyMe.Api` (class libs / web project as appropriate).
- [x] Create `tests/NotifyMe.Domain.Tests`, `tests/NotifyMe.Application.Tests`,
      `tests/NotifyMe.IntegrationTests` (xUnit).
- [x] Wire project references: `Application` -> `Domain`; `Infrastructure` -> `Application` +
      `Domain`; `Api` -> `Application` + `Infrastructure` + `Domain`. `Domain` references
      nothing. Verified by counting `ProjectReference` entries per csproj.
- [x] Add `Directory.Build.props` for shared settings (nullable enable, treat warnings as
      errors, analyzers).
- [x] Add `docker-compose.yml` with `postgres` and `mailhog` services; validated with
      `docker compose config`.
- [x] Removed template placeholder files (`Class1.cs`, `UnitTest1.cs`, sample
      `WeatherForecast` minimal API endpoint) so the scaffold doesn't carry unrelated sample
      code into Phase 03+.
- [ ] Base NuGet packages per layer (EF Core/Npgsql, MailKit, FluentValidation, Polly, Serilog,
      Testcontainers, WireMock.Net, etc.) are deferred to the phases that actually need them
      (05, 06, 07, 09, 10, 11) rather than added speculatively here.

## Verification
- `dotnet build` succeeds with all 7 projects (confirmed).
- `docker compose config` validates the compose file syntax (confirmed; full `docker compose up`
  smoke test deferred to Phase 05 when persistence code actually needs the database).
- Project reference graph matches the Clean Architecture dependency rule (confirmed by counting
  `ProjectReference` entries: Application=1, Infrastructure=2, Api=3, Domain.Tests=1,
  Application.Tests=2, IntegrationTests=1).

## Status
Done (2026-09-22).
