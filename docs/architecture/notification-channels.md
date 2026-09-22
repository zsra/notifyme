# Notification channels

## The abstraction

`INotificationChannel` is declared in the `Domain` layer:

```
Task<NotificationResult> SendAsync(NotificationContext context, CancellationToken ct);
```

`NotificationContext` carries whatever a channel needs to render and send a message (the
matched event, the alert rule, the target address/webhook URL from `ChannelConfig`).

## Why a string-keyed registry instead of a closed enum

The brief explicitly asks for a system "flexible enough that we can add more channels later."
A closed `enum ChannelType { Slack, Email }` would mean every new channel requires a change to a
shared core type. Instead, `ChannelConfig.ChannelType` is a string discriminator (e.g. `"slack"`,
`"email"`), and every `INotificationChannel` implementation exposes its own `ChannelType` string.
`DispatchNotificationUseCase` (in `Application`) takes the whole set of registered channels as
`IEnumerable<INotificationChannel>` via DI and builds a `ChannelType -> INotificationChannel`
lookup from it - there is no separate `ChannelResolver` type; DI's native "resolve all
registrations of an interface" behavior plus that one dictionary *is* the registry. Adding a
channel means:

1. Implement `INotificationChannel` (e.g. `TeamsNotificationChannel`) in `Infrastructure`.
2. Register it in DI as another `INotificationChannel`, in `Infrastructure`/`Api` composition
   only (see `NotificationChannelsServiceCollectionExtensions.AddNotifyMeNotificationChannels`).
3. No changes to `Domain`, `Application`, or existing channel implementations.

## Current implementations

- **`SlackNotificationChannel`**: posts a JSON payload to a configured Slack Incoming Webhook URL
  (the `ChannelConfig.Target`) via a typed `HttpClient`. No OAuth/bot token required.
- **`EmailNotificationChannel`**: builds the message and delegates the actual SMTP conversation
  to an `IEmailSender` seam (`SmtpEmailSender` in production, a hand-rolled fake in unit tests),
  sent via MailKit. Locally this points at MailHog (run via `docker-compose`) so delivery can be
  verified visually without a real mailbox or credentials.

## Configuration and secrets

SMTP settings live under the `NotificationChannels:Email` appsettings section
(`src/NotifyMe.Api/appsettings.json`); the Slack webhook URL is per-`ChannelConfig` data (its
`Target`), not app-wide configuration. Real webhook URLs and any production SMTP credentials are
sourced from `dotnet user-secrets` locally and environment variables in CI/deployment, never
committed; docs and examples use obviously-fake placeholders (e.g.
`https://hooks.slack.com/services/PLACEHOLDER`).

## Known limitation

Only Slack and Email exist today. The registry/abstraction is the deliverable that proves
extensibility, not an exhaustive channel list.
