# Phase 14 - Frontend admin screens

## Goal
Implement the actual CRUD/read screens for Alert Rules, Channels, Subscriptions, and
Notification history, plus the manual trigger-simulated-event action, on top of Phase 13's
foundation. This is the phase that makes the frontend actually useful as an admin view.

## Depends on
Phase 13.

## Steps
- [x] Alert Rules screen: list (filterable by `eventCategory`/`isEnabled`), create, edit, delete.
- [x] Channels screen: list (filterable by `channelType`/`isEnabled`), create, edit, delete.
- [x] Subscriptions screen: list (filterable by `alertRuleId`/`channelConfigId`), create (link a
      rule to a channel via selects populated from the other two screens' data), delete.
- [x] Notifications screen: read-only list with `status`/`alertRuleId`/`sentFrom`/`sentTo`
      filters; no create/edit/delete controls, matching the read-only API.
- [x] "Trigger simulated event" action (calls `POST /api/admin/events/trigger-simulated`) with a
      simple success/failure indicator.
- [x] Consistent handling of API validation errors (render `ProblemDetails.detail` next to the
      relevant form/action) and of `401` responses (send the user back to the API key entry
      screen from Phase 13).
- [x] TanStack Query cache invalidation wired so mutations (create/update/delete) refresh the
      relevant list(s) without a manual page reload.

## Verification
- Manual walkthrough entirely through the UI: create a Channel, create an Alert Rule, create a
  Subscription linking them, trigger a simulated event, and see a new entry appear in
  Notifications history.
- Deleting a resource that the API rejects (e.g. a referenced Alert Rule) surfaces the API's
  actual error response in the UI rather than failing silently or crashing.

## Design notes

- **Shared plumbing added this phase**: `src/api/types.ts` (re-exports of `components["schemas"]`
  DTOs so pages don't reach into the generated schema directly), `src/api/apiError.ts` (an
  `ApiError`/`unwrap()` pair that turns `openapi-fetch`'s `{ data, error, response }` result into
  a thrown error on non-2xx responses, with the message taken from `ProblemDetails.detail`), and
  `src/components/ErrorMessage.tsx` (renders `error.message` next to whichever form/action
  triggered it). TanStack Query's `error` field from `useQuery`/`useMutation` is rendered directly
  through this component - no separate error-state plumbing needed.
- **Real API constraint discovered, not a bug**: `UpdateAlertRuleRequest` and
  `UpdateChannelConfigRequest` have no `isEnabled` field (see `AlertRulesEndpoints.cs`/
  `ChannelsEndpoints.cs` - only `Create*Request` has it). The edit forms only show the "Enabled"
  checkbox when creating a new row; editing an existing Alert Rule/Channel can change its other
  fields but not toggle enabled/disabled after creation. This is a real gap in the Admin API
  itself (Phase 08), out of scope for the frontend to silently paper over - a future backend
  change to add an explicit enable/disable endpoint would need its own ADR/phase.
  `isEnabled`/`channelType` filters still work for browsing existing data either way.
- **Subscriptions/Notifications reference Alert Rules and Channels by GUID only**
  (`SubscriptionDto`/`NotificationDto` don't inline the related name), so both pages fetch the
  full unfiltered Alert Rules/Channels lists client-side and look up names for display, falling
  back to the raw GUID if the referenced row no longer exists (verified against a long-lived
  local dev database with orphaned subscriptions/notifications from earlier manual testing -
  the fallback renders sensibly rather than crashing or showing `undefined`).
- **Channel type is a free string**, not an enum (`ChannelConfig.ChannelType` is a string
  discriminator per `docs/architecture/notification-channels.md`), so the Channels screen uses a
  plain `<input list="known-channel-types">` with a `<datalist>` suggesting `slack`/`email`
  rather than a `<select>` - any value the backend has a registered `INotificationChannel` for
  still works, this is just a typing convenience.
- **"Trigger simulated event"** lives on the Status page (`StatusPage.tsx`) rather than its own
  route, since it's a cross-cutting operational action (identical to what `EventIngestionWorker`
  does automatically on its polling interval), not tied to a single resource screen; it renders
  the typed `IngestEventsResult` fields (`rawEventCount`/`normalizedEventCount`/
  `notificationsDispatched`) on success.
- **Delete confirmation** uses the browser's native `window.confirm()`, consistent with "no
  UI framework, minimal inline styles" - a custom modal would be pure visual polish with no
  technical value for this exercise.
- **Manually verified end-to-end** with the API, Postgres, and Vite dev server all running
  locally: created a Channel, an Alert Rule (with keywords), and a Subscription linking them
  through the actual UI (not just API calls); edited the Alert Rule's keywords and confirmed the
  table and cache updated without a reload; used "Trigger now" and saw the typed
  `IngestEventsResult` render; confirmed the Notifications screen's status filter re-queries
  correctly; deleted all three test resources back out through the UI (delete confirmation
  dialogs handled correctly). `npm run build` (`tsc -b && vite build`) passes with zero
  TypeScript errors.

## Status
Done (2026-09-22).

