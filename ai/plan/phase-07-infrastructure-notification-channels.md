# Phase 07 - Infrastructure: notification channels

## Goal
Implement real, locally-verifiable Slack and Email delivery behind the `INotificationChannel`
abstraction, plus a registry so new channels can be added without touching core domain types.

## Depends on
Phase 04 (`INotificationChannel` interface must exist). Can be worked in parallel with Phase 06.

## Steps
- [ ] Implement `SlackNotificationChannel`: HTTP POST to a configured Incoming Webhook URL.
- [ ] Implement `EmailNotificationChannel`: MailKit SMTP client, pointed at MailHog locally.
- [ ] Implement a `ChannelResolver`/registry keyed by a string channel-type discriminator (not a
      closed enum) so a new channel is "add one class + one DI registration", per
      `docs/architecture/notification-channels.md`.
- [ ] Unit tests mocking the HTTP client / SMTP client.
- [ ] A manual/integration check: send a real message end-to-end and confirm it's visible in the
      MailHog web UI (email) and captured via WireMock.Net (Slack).

## Verification
- Adding a hypothetical new channel requires no changes to `Domain` or `Application`.
- Unit tests pass; manual MailHog/WireMock check confirms real delivery works locally.

## Status
Not started.
