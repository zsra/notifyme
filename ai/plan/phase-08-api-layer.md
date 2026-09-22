# Phase 08 - API layer

## Goal
Expose the Admin API: CRUD for alert rules, subscriptions, and channels, read access to
notification history, and a manual "trigger a simulated event now" endpoint for demoability, all
protected by a simple API key header.

## Depends on
Phases 05, 06, 07 (persistence, event ingestion, and channels must all exist).

## Steps
- [x] Minimal API (or controllers) for `AlertRules`, `Subscriptions`, `Channels` CRUD.
- [x] Read-only endpoint for `Notifications` history with basic filtering (status, date range).
- [x] `POST /api/admin/events/trigger-simulated` for manual demo triggering.
- [x] API key auth middleware reading the expected key from configuration/user-secrets.
- [x] Request/response DTOs distinct from domain entities; `ProblemDetails`-based error
      responses.
- [x] Swagger/OpenAPI generation.

## Verification
- Swagger UI is reachable and documents all endpoints.
- Manual end-to-end via curl/Postman: create a rule, trigger a simulated event, see it land in
  notification history, and confirm delivery in MailHog/WireMock.
- Requests without a valid API key are rejected.

## Design notes / course-corrections
- The Application layer (Phase 04) only had use cases for the flows exercised so far
  (create/update AlertRule, dispatch notification, etc.). Full CRUD needed several new use cases
  that didn't exist yet: `GetAlertRuleUseCase`, `ListAlertRulesUseCase`, `DeleteAlertRuleUseCase`;
  a whole new `Channels` slice (`CreateChannelConfigUseCase`, `UpdateChannelConfigUseCase`,
  `DeleteChannelConfigUseCase`, `GetChannelConfigUseCase`, `ListChannelConfigsUseCase` plus DTOs
  and FluentValidation validators - `ChannelConfig` had no CRUD use cases at all before this
  phase); `ListSubscriptionsUseCase`; and `NotificationDto` plus `GetNotificationUseCase`/
  `ListNotificationsUseCase`. Also added `ChannelConfig.UpdateDetails(...)` to the Domain entity
  (mirroring `AlertRule.UpdateDetails`) so the update use case can edit type/target in place.
- Added `ApplicationServiceCollectionExtensions.AddNotifyMeApplication()` (new
  `Microsoft.Extensions.DependencyInjection.Abstractions` package reference in
  `NotifyMe.Application`) registering all validators and use cases as `Scoped`, mirroring the
  per-layer `AddNotifyMeXxx` pattern already used by Infrastructure.
- API key auth is an `IEndpointFilter` (`ApiKeyEndpointFilter`), not ASP.NET Core authentication
  middleware, reading `X-Api-Key` and comparing against `Admin:ApiKey` config via
  `CryptographicOperations.FixedTimeEquals`. Applied to `app.MapGroup("/api/admin")`. Fails closed
  (401) if the server itself has no key configured.
- Errors are mapped via an `IExceptionHandler` (`NotifyMeExceptionHandler`): `NotFoundException`
  to 404, FluentValidation's `ValidationException` and `ArgumentException`/
  `ArgumentOutOfRangeException` to 400, all as RFC 7807 `ProblemDetails` with a `traceId`
  extension.
- Deliberately did **not** implement the draft doc's "optionally accepts an `eventCategory`"
  parameter on `trigger-simulated`: `IEventSource` has no notion of fetching only one category,
  and adding one just for this demo endpoint would leak a simulator-specific concern into the
  Domain abstraction. `docs/api/admin-api.md` corrected to match.
- Minimal APIs used throughout (5 endpoint-group files under `src/NotifyMe.Api/Endpoints/`), each
  exposing a `MapXxxEndpoints(IEndpointRouteBuilder)` extension; PUT endpoints use the
  `request with { Id = id }` record-`with` trick to force the route id over any body-supplied id.
  `Program.cs` rewritten as the full composition root (persistence, event source, notification
  channels, application services, API-key filter, exception handler, all 5 endpoint groups).
- New integration test suite under `tests/NotifyMe.IntegrationTests/Api/` using
  `Microsoft.AspNetCore.Mvc.Testing`'s `WebApplicationFactory<Program>` (new package reference,
  version 10.0.0, resolved with no conflicts): API-key rejection tests, full CRUD round trips for
  AlertRules/Channels/Subscriptions, and a deterministic end-to-end trigger-to-dispatch test using
  three alert rules (one per `EventCategory`, matching via empty keywords + `Severity.Medium`)
  subscribed to a WireMock-backed Slack channel, so the test is not flaky despite
  `SimulatedEventSource`'s randomness.
- Full-solution `dotnet test` after all Phase 08 work: 102/102 tests passing.

## Status
Done (2026-09-22).
