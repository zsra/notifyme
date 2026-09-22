# ADR-0003: PostgreSQL via docker-compose for local development

- **Status**: Accepted
- **Date**: 2026-09-22

## Context
Alert rules, subscriptions, channel configs, and notification history need durable, queryable
storage with real migrations support.

## Decision
Use PostgreSQL as the primary datastore, run locally via `docker-compose`, accessed through EF
Core with the Npgsql provider.

## Alternatives considered
- SQLite (file-based) - zero setup, good for a quick demo, but weaker migrations/concurrency
  story and less representative of a production-grade choice.
- SQL Server (LocalDB or Docker) - familiar in many .NET shops, but heavier tooling and licensing
  considerations for a project with no Windows/SQL Server-specific requirement.

## Consequences
Requires Docker locally (and in CI, for integration tests via Testcontainers). Gives a
realistic, production-representative persistence story and lets integration tests exercise a
real Postgres instance rather than an in-memory substitute.
