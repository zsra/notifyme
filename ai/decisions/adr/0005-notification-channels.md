# ADR-0005: Slack Incoming Webhook + SMTP (MailKit) against a local dev SMTP catcher

- **Status**: Accepted
- **Date**: 2026-09-22

## Context
The brief requires both email and Slack notification channels, working "for real" (not just
logging), while remaining testable locally without production credentials or paid services.

## Decision
Implement Slack delivery via Incoming Webhook (simple HTTP POST, no OAuth needed) and email
delivery via SMTP using MailKit, pointed at a local dev SMTP catcher (MailHog) run through
`docker-compose`. Both sit behind a shared `INotificationChannel` abstraction with a
string-keyed channel registry (not a closed enum) so a new channel is "add one class + one DI
registration."

## Alternatives considered
- Slack Incoming Webhook + SendGrid API for email - more production-realistic, but requires a
  SendGrid account/API key, adding an external dependency for no added evaluation signal.
- Interface-only stubs (console/log channels) - fastest, but doesn't satisfy the brief's request
  for actual working notification delivery.

## Consequences
Local development requires `docker-compose` (for MailHog) and a real or placeholder Slack
webhook URL (never committed; supplied via user-secrets/environment variables). Delivery can be
verified visually (MailHog web UI) and via captured HTTP calls (WireMock.Net in tests) without
touching any real external service.
