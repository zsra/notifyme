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
`"email"`), and a `ChannelResolver` in `Infrastructure` maps that string to a registered
`INotificationChannel` implementation via DI. Adding a channel means:

1. Implement `INotificationChannel` (e.g. `TeamsNotificationChannel`).
2. Register it in DI keyed by its string type, in `Infrastructure`/`Api` composition only.
3. No changes to `Domain`, `Application`, or existing channel implementations.

## Current implementations

- **`SlackNotificationChannel`**: posts a JSON payload to a configured Slack Incoming Webhook URL
  via `HttpClient`. No OAuth/bot token required.
- **`EmailNotificationChannel`**: sends via SMTP using MailKit. Locally, this points at MailHog
  (run via `docker-compose`) so delivery can be verified visually without a real mailbox or
  credentials.

## Configuration and secrets

Webhook URLs and SMTP settings are supplied via configuration, sourced from `dotnet user-secrets`
locally and environment variables in CI/deployment. Real values are never committed; docs and
examples use obviously-fake placeholders (e.g. `https://hooks.slack.com/services/PLACEHOLDER`).

## Known limitation

Only Slack and Email exist today. The registry/abstraction is the deliverable that proves
extensibility, not an exhaustive channel list.
