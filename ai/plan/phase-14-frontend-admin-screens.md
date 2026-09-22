# Phase 14 - Frontend admin screens

## Goal
Implement the actual CRUD/read screens for Alert Rules, Channels, Subscriptions, and
Notification history, plus the manual trigger-simulated-event action, on top of Phase 13's
foundation. This is the phase that makes the frontend actually useful as an admin view.

## Depends on
Phase 13.

## Steps
- [ ] Alert Rules screen: list (filterable by `eventCategory`/`isEnabled`), create, edit, delete.
- [ ] Channels screen: list (filterable by `channelType`/`isEnabled`), create, edit, delete.
- [ ] Subscriptions screen: list (filterable by `alertRuleId`/`channelConfigId`), create (link a
      rule to a channel via selects populated from the other two screens' data), delete.
- [ ] Notifications screen: read-only list with `status`/`alertRuleId`/`sentFrom`/`sentTo`
      filters; no create/edit/delete controls, matching the read-only API.
- [ ] "Trigger simulated event" action (calls `POST /api/admin/events/trigger-simulated`) with a
      simple success/failure indicator.
- [ ] Consistent handling of API validation errors (render `ProblemDetails.detail` next to the
      relevant form/action) and of `401` responses (send the user back to the API key entry
      screen from Phase 13).
- [ ] TanStack Query cache invalidation wired so mutations (create/update/delete) refresh the
      relevant list(s) without a manual page reload.

## Verification
- Manual walkthrough entirely through the UI: create a Channel, create an Alert Rule, create a
  Subscription linking them, trigger a simulated event, and see a new entry appear in
  Notifications history.
- Deleting a resource that the API rejects (e.g. a referenced Alert Rule) surfaces the API's
  actual error response in the UI rather than failing silently or crashing.

## Status
Not started.
