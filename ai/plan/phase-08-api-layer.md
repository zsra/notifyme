# Phase 08 - API layer

## Goal
Expose the Admin API: CRUD for alert rules, subscriptions, and channels, read access to
notification history, and a manual "trigger a simulated event now" endpoint for demoability, all
protected by a simple API key header.

## Depends on
Phases 05, 06, 07 (persistence, event ingestion, and channels must all exist).

## Steps
- [ ] Minimal API (or controllers) for `AlertRules`, `Subscriptions`, `Channels` CRUD.
- [ ] Read-only endpoint for `Notifications` history with basic filtering (status, date range).
- [ ] `POST /api/admin/events/trigger-simulated` for manual demo triggering.
- [ ] API key auth middleware reading the expected key from configuration/user-secrets.
- [ ] Request/response DTOs distinct from domain entities; `ProblemDetails`-based error
      responses.
- [ ] Swagger/OpenAPI generation.

## Verification
- Swagger UI is reachable and documents all endpoints.
- Manual end-to-end via curl/Postman: create a rule, trigger a simulated event, see it land in
  notification history, and confirm delivery in MailHog/WireMock.
- Requests without a valid API key are rejected.

## Status
Not started.
