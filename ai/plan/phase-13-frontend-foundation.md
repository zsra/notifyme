# Phase 13 - Frontend foundation & API integration

## Goal
Scaffold a React/TypeScript admin frontend and wire it end-to-end against the existing Admin
API (auth, typed client, health check), with no business/CRUD screens yet beyond a minimal shell
that proves the plumbing works. See ADR-0009 for the stack decisions this phase implements.

## Depends on
Phase 08 (Admin API contract must be stable and unchanged since).

## Steps
- [ ] Scaffold `frontend/` with Vite + React + TypeScript (npm).
- [ ] Add a CORS policy to `NotifyMe.Api` allowing a configured local frontend origin (e.g.
      `http://localhost:5173`), sourced from configuration, not hardcoded.
- [ ] Generate TypeScript types for the Admin API DTOs from the API's own OpenAPI document
      (`openapi-typescript` or equivalent); check in the generated output with a documented
      regeneration command (e.g. an npm script), never hand-edited.
- [ ] Thin typed API client (fetch wrapper) that attaches `X-Api-Key` from an auth context/hook,
      and parses `ProblemDetails` error responses into a consistent shape for the UI.
- [ ] API key entry screen storing the key in `sessionStorage` (see ADR-0009); the app shell
      blocks Admin API calls and redirects back to this screen until a key is set, and again on
      a `401` response.
- [ ] Minimal routing shell (React Router) with placeholder nav links for Alert Rules, Channels,
      Subscriptions, and Notifications, plus a status page/indicator hitting `GET /health`.
- [ ] Local dev instructions (README addendum or `frontend/README.md`): `npm install`,
      `npm run dev`, the type-regeneration command, and the required `NOTIFYME_CORS_ORIGIN`
      (or equivalent) backend setting.

## Verification
- `npm run build` succeeds with no TypeScript errors.
- With the backend running locally (`docker compose up -d`, `dotnet run --project
  src/NotifyMe.Api`), the Vite dev server: prompts for and stores an API key, then successfully
  calls `GET /health` and renders its status, with no CORS error in the browser console.
- Regenerating the OpenAPI-derived types against the current running API produces no diff (the
  contract hasn't drifted since Phase 08).

## Status
Not started.
