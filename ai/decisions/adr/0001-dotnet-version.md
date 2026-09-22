# ADR-0001: Target .NET 10 (LTS)

- **Status**: Accepted
- **Date**: 2026-09-22

## Context
The brief allows any technology/language/framework. The project needs a runtime that will
realistically be supported for the lifetime of this exercise and any follow-up work, without
needing to chase a short support window.

## Decision
Target .NET 10 (LTS) for the backend.

## Alternatives considered
- .NET 8 (LTS) - safe and stable, but not the current LTS in 2026; would mean starting a new
  project on an older baseline for no real benefit.
- .NET 9 (STS) - latest features at the time it shipped, but a short support window makes it a
  weaker choice for a project meant to demonstrate durable engineering decisions.

## Consequences
Requires the .NET 10 SDK to be installed locally and in CI. No known blockers; standard ASP.NET
Core / EF Core tooling is expected to fully support it.
