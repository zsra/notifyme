# Phase 07 - Infrastructure: notification channels

## Goal
Implement real, locally-verifiable Slack and Email delivery behind the `INotificationChannel`
abstraction, plus a registry so new channels can be added without touching core domain types.

## Depends on
Phase 04 (`INotificationChannel` interface must exist). Can be worked in parallel with Phase 06.

## Steps
- [x] Implement `SlackNotificationChannel`: HTTP POST to a configured Incoming Webhook URL.
- [x] Implement `EmailNotificationChannel`: MailKit SMTP client, pointed at MailHog locally.
- [x] Implement a `ChannelResolver`/registry keyed by a string channel-type discriminator (not a
      closed enum) so a new channel is "add one class + one DI registration", per
      `docs/architecture/notification-channels.md`.
- [x] Unit tests mocking the HTTP client / SMTP client.
- [x] A manual/integration check: send a real message end-to-end and confirm it's visible in the
      MailHog web UI (email) and captured via WireMock.Net (Slack).

## Verification
- Adding a hypothetical new channel requires no changes to `Domain` or `Application`.
- Unit tests pass; manual MailHog/WireMock check confirms real delivery works locally.

## Design notes / course-corrections
- No separate `ChannelResolver` type was added. `DispatchNotificationUseCase` (Application,
  already written in Phase 04) takes `IEnumerable<INotificationChannel>` via DI and builds the
  `ChannelType -> INotificationChannel` dictionary itself - that already *is* the registry the
  brief asks for. Fixed a doc drift in `docs/architecture/notification-channels.md` that
  described a standalone `ChannelResolver` type that was never actually planned to exist
  separately from that use case.
- `SlackNotificationChannel` is a typed `HttpClient` (`services.AddHttpClient<SlackNotificationChannel>()`)
  that POSTs `{ "text": "..." }` to `ChannelConfig.Target` (the webhook URL itself). Failures
  (non-2xx response, `HttpRequestException`, timeout) are caught and converted to
  `NotificationDispatchResult.Failure(...)` rather than thrown, matching
  `DispatchNotificationUseCase`'s "every outcome is a terminal `Notification` state" contract.
- `EmailNotificationChannel` builds the `MimeMessage` and delegates the actual SMTP
  connect/send/disconnect to a small `IEmailSender` seam (`SmtpEmailSender` in production). This
  seam exists specifically so unit tests can use a hand-rolled fake `IEmailSender` instead of
  needing to implement MailKit's much larger `ISmtpClient` interface or spin up a real SMTP
  server - consistent with this repo's existing "hand-rolled test double" convention
  (`FakeEventSource`, `FakeNotificationChannel`) over a mocking framework.
- SMTP settings (`NotificationChannels:Email` appsettings section: host/port/from
  address/`UseSsl`) are app-wide configuration; the Slack webhook URL is per-`ChannelConfig` data
  (`Target`), not appsettings, since each Slack-type subscription can point at a different
  webhook.
- Unit tests (`NotifyMe.Infrastructure.Tests`) mock the HTTP client via a hand-rolled
  `FakeHttpMessageHandler` (Slack) and the SMTP client via a hand-rolled `FakeEmailSender`
  (Email) - no mocking library added, consistent with the rest of the test suite.
- The manual/integration check lives in `tests/NotifyMe.IntegrationTests/NotificationChannels/`:
  one test spins up a local WireMock.Net server as a stand-in Slack webhook and asserts the POST
  was actually received with the expected payload; the other sends a real email over SMTP to the
  docker-compose MailHog instance and confirms arrival via MailHog's own HTTP API
  (`/api/v2/search`). Both require `docker compose up -d mailhog` (and Postgres, for the
  unrelated Phase 05 test in the same project) running first. A broader, more end-to-end
  (API-driven) version of this is planned for Phase 11.
- Hit and resolved a real dependency conflict: the latest `WireMock.Net` (2.x) bundles an
  `OpenApiParser` feature that hard-requires `Microsoft.OpenApi >= 3.10.2`, which broke
  `Microsoft.AspNetCore.OpenApi`'s source generator (compiled against `Microsoft.OpenApi 2.0.0`)
  wherever `NotifyMe.IntegrationTests` (which references `NotifyMe.Api`) was compiled. Pinning
  `Microsoft.OpenApi` down was rejected by NuGet as a downgrade error given WireMock's hard
  minimum. Fixed by pinning `WireMock.Net` to `1.25.0` (the last 1.x release, predating that
  bundled dependency) instead, which resolves cleanly.

## Status
Done (2026-09-22).
