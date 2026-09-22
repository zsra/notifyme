# ADR-0006: Admin API scope and API-key auth for this phase

- **Status**: Accepted
- **Date**: 2026-09-22

## Context
The brief asks for "an admin view" with no detail on scope or who should access it. A full
auth/identity story is out of scope for the backend-first phase (frontend and its auth strategy
are being discussed separately), but leaving the admin surface completely open even in a local
dev environment is a bad default.

## Decision
Scope the Admin API to CRUD for AlertRules, Subscriptions, and Channels, plus read-only
notification history and a manual "trigger simulated event" endpoint. Protect all admin routes
with a simple API key supplied via a request header, sourced from configuration/user-secrets.

## Alternatives considered
- No auth at all for this phase - simplest, but leaves even a local/dev deployment fully open;
  rejected as a bad default even for a demo project.
- Read-only admin endpoints only, deferring write operations - reduces scope further, but the
  brief's core ask (manage alerts) requires write operations to be meaningfully demoable.

## Consequences
API key auth is explicitly a dev-grade placeholder, not a production auth story. This is a known
limitation to revisit once the frontend and its identity requirements are scoped. The key itself
must never be committed; it is supplied via user-secrets locally and environment variables in
CI/deployment.
