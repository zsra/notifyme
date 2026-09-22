# Phase 11 - Testing hardening

## Goal
Round out the test suite so the full pipeline is verified automatically, not just by manual
checks.

## Depends on
Phase 09 (pipeline must be functionally complete).

## Steps
- [ ] Integration tests using `WebApplicationFactory` + `Testcontainers.PostgreSql` for the
      Admin API against a real (ephemeral) database.
- [ ] WireMock.Net-based integration tests confirming Slack webhook payloads.
- [ ] MailHog-based (or equivalent) integration tests confirming email delivery and content.
- [ ] Generate and review a coverage report; look for untested branches in the matching logic
      and channel dispatch retry paths specifically.

## Verification
- Full `dotnet test` suite passes locally in a clean environment (fresh containers).
- Coverage report reviewed; gaps in critical paths (matching, dispatch, auth) are either closed
  or explicitly noted as accepted risk.

## Status
Not started.
