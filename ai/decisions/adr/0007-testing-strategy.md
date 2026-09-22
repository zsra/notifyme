# ADR-0007: Unit + integration testing strategy

- **Status**: Accepted
- **Date**: 2026-09-22

## Context
The project needs enough automated test coverage to demonstrate engineering rigor without
spending the entire time budget on test infrastructure.

## Decision
Use xUnit for unit tests covering `Domain` and `Application` logic (especially alert-matching
rules). Use integration tests built on `WebApplicationFactory` plus Testcontainers for
PostgreSQL, WireMock.Net for capturing Slack webhook calls, and MailHog for verifying email
delivery, covering the API and infrastructure layers end-to-end.

## Alternatives considered
- Unit tests only - faster, but leaves the API/DB/channel wiring unverified by automation,
  relying on manual checks alone.
- Minimal smoke tests only - fastest, but provides the weakest evidence of correctness and
  regression safety, which cuts against the project's evaluation focus on rigor and validation.

## Consequences
Integration tests require Docker (Testcontainers) to run, both locally and in CI. This is a
reasonable trade-off given PostgreSQL is already a docker-compose dependency (ADR-0003).
