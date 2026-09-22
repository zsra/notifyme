# Admin API

Status: implemented in Phase 08. This reflects the actual API as built.

## Auth

All `/api/admin/*` routes require an `X-Api-Key` header. The expected key is sourced from
configuration/user-secrets; requests without a valid key get `401 Unauthorized`. See ADR-0006.

## Endpoints

### Alert rules

- `GET /api/admin/alert-rules` - list, filterable by `eventCategory`, `isEnabled`.
- `GET /api/admin/alert-rules/{id}`
- `POST /api/admin/alert-rules` - create.
- `PUT /api/admin/alert-rules/{id}` - update.
- `DELETE /api/admin/alert-rules/{id}`

### Channels

- `GET /api/admin/channels` - list, filterable by `channelType`, `isEnabled`.
- `GET /api/admin/channels/{id}`
- `POST /api/admin/channels` - create (e.g. a Slack webhook target or an email address).
- `PUT /api/admin/channels/{id}`
- `DELETE /api/admin/channels/{id}`

### Subscriptions

- `GET /api/admin/subscriptions` - list, filterable by `alertRuleId`, `channelConfigId`.
- `POST /api/admin/subscriptions` - link an alert rule to a channel.
- `DELETE /api/admin/subscriptions/{id}`

### Notifications (read-only history)

- `GET /api/admin/notifications` - filterable by `status`, `alertRuleId`, `sentFrom`, `sentTo`
  (all optional query parameters). Note: `sentFrom`/`sentTo` only match notifications that have a
  non-null `SentAt`, since `Notification` has no separate "created at" timestamp.
- `GET /api/admin/notifications/{id}`

### Demo/testing helper

- `POST /api/admin/events/trigger-simulated` - forces the `SimulatedEventSource` to produce and
  ingest one event immediately, for demoing the pipeline without waiting on the polling
  interval. Does **not** accept a parameter to force a specific `eventCategory`: the
  `IEventSource` abstraction has no notion of "fetch only this category", and adding one just for
  this demo endpoint would leak a simulator-specific concern into the Domain abstraction (see
  [docs/architecture/event-ingestion.md](../architecture/event-ingestion.md)).

## Error format

Errors use RFC 7807 `ProblemDetails` with a consistent shape:

```json
{
  "type": "https://example.com/errors/validation-failed",
  "title": "Validation failed",
  "status": 400,
  "detail": "AlertRule.Name is required.",
  "traceId": "..."
}
```

## OpenAPI / Swagger

The API exposes an OpenAPI document via `AddOpenApi()`/`MapOpenApi()` (development environment
only), following the default ASP.NET Core minimal-API template pattern.

## Notes

DTOs deliberately mirror but do not reuse domain entities directly (avoid leaking persistence
concerns through the API): `AlertRuleDto`, `ChannelConfigDto`, `SubscriptionDto`, `NotificationDto`.
Auth is implemented as an `IEndpointFilter` (`ApiKeyEndpointFilter`) applied to the
`/api/admin` route group, not ASP.NET Core authentication middleware; it fails closed (401) if the
server has no `Admin:ApiKey` configured. Errors are mapped via an `IExceptionHandler`
(`NotifyMeExceptionHandler`).
