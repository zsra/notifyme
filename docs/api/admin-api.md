# Admin API (draft contract)

Status: draft, written before any code exists (Phase 01). Will be corrected against the actual
implementation at the end of Phase 08.

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

- `GET /api/admin/notifications` - filterable by `status`, `alertRuleId`, date range.
- `GET /api/admin/notifications/{id}`

### Demo/testing helper

- `POST /api/admin/events/trigger-simulated` - forces the `SimulatedEventSource` to produce and
  ingest one event immediately, for demoing the pipeline without waiting on the polling
  interval. Optionally accepts an `eventCategory` to force a specific category.

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

## Notes

DTOs deliberately mirror but do not reuse domain entities directly (avoid leaking persistence
concerns through the API). Exact DTO field names will be finalized in Phase 08 alongside the
implementation.
