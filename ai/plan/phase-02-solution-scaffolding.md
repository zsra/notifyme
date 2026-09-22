# Phase 02 - Solution & project scaffolding

## Goal
Create the actual .NET solution structure with the four Clean Architecture projects and three
test projects, wired with the correct dependency direction, plus the local dev infrastructure
(docker-compose for Postgres + MailHog).

## Depends on
Phase 01 (design docs must exist so project boundaries and namespaces are chosen deliberately,
not guessed).

## Steps
- [ ] `dotnet new sln` at repo root (`NotifyMe.sln`).
- [ ] Create `src/NotifyMe.Domain`, `src/NotifyMe.Application`, `src/NotifyMe.Infrastructure`,
      `src/NotifyMe.Api` (class libs / web project as appropriate).
- [ ] Create `tests/NotifyMe.Domain.Tests`, `tests/NotifyMe.Application.Tests`,
      `tests/NotifyMe.IntegrationTests` (xUnit).
- [ ] Wire project references: `Application` -> `Domain`; `Infrastructure` -> `Application` +
      `Domain`; `Api` -> `Application` + `Infrastructure` + `Domain`. `Domain` references
      nothing.
- [ ] Add `Directory.Build.props` for shared settings (nullable enable, treat warnings as
      errors, analyzers).
- [ ] Add `docker-compose.yml` with `postgres` and `mailhog` services.
- [ ] Add base NuGet packages per layer (deferred to actual implementation, not decided in
      detail here).

## Verification
- `dotnet build` succeeds with all projects (empty scaffolds compile).
- `docker compose up` brings up Postgres and MailHog and both are reachable.
- Project reference graph matches the Clean Architecture dependency rule (verifiable via
  `dotnet list reference` on each project).

## Status
Not started. No code exists yet.
