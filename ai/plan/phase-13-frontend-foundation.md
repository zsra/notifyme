# Phase 13 - Frontend foundation & API integration

## Goal
Scaffold a React/TypeScript admin frontend and wire it end-to-end against the existing Admin
API (auth, typed client, health check), with no business/CRUD screens yet beyond a minimal shell
that proves the plumbing works. See ADR-0009 for the stack decisions this phase implements.

## Depends on
Phase 08 (Admin API contract must be stable and unchanged since).

## Steps
- [x] Scaffold `frontend/` with Vite + React + TypeScript (npm).
- [x] Add a CORS policy to `NotifyMe.Api` allowing a configured local frontend origin (e.g.
      `http://localhost:5173`), sourced from configuration, not hardcoded.
- [x] Generate TypeScript types for the Admin API DTOs from the API's own OpenAPI document
      (`openapi-typescript` or equivalent); check in the generated output with a documented
      regeneration command (e.g. an npm script), never hand-edited.
- [x] Thin typed API client (fetch wrapper) that attaches `X-Api-Key` from an auth context/hook,
      and parses `ProblemDetails` error responses into a consistent shape for the UI.
- [x] API key entry screen storing the key in `sessionStorage` (see ADR-0009); the app shell
      blocks Admin API calls and redirects back to this screen until a key is set, and again on
      a `401` response.
- [x] Minimal routing shell (React Router) with placeholder nav links for Alert Rules, Channels,
      Subscriptions, and Notifications, plus a status page/indicator hitting `GET /health`.
- [x] Local dev instructions (README addendum or `frontend/README.md`): `npm install`,
      `npm run dev`, the type-regeneration command, and the required `NOTIFYME_CORS_ORIGIN`
      (or equivalent) backend setting.

## Verification
- `npm run build` succeeds with no TypeScript errors.
- With the backend running locally (`docker compose up -d`, `dotnet run --project
  src/NotifyMe.Api`), the Vite dev server: prompts for and stores an API key, then successfully
  calls `GET /health` and renders its status, with no CORS error in the browser console.
- Regenerating the OpenAPI-derived types against the current running API produces no diff (the
  contract hasn't drifted since Phase 08).

## Design notes

- **Scaffolding**: `npm create vite@latest frontend -- --template react-ts` (non-interactively,
  since the interactive linter prompt can't be driven through this tool's terminal). Removed the
  template's demo content (`App.css`, hero/react/vite assets, the counter button) and replaced
  `index.css` with a handful of lines, consistent with the "technical, not visual" priority.
- **CORS**: Added a `Cors:FrontendOrigin` config key (`appsettings.json`, default
  `http://localhost:5173`) and a named CORS policy in `Program.cs`, applied via `app.UseCors(...)`
  before endpoint mapping. Configuration-driven rather than hardcoded, per ADR-0009.
- **OpenAPI response types were missing**: discovered that none of the Admin API's endpoints had
  typed response schemas in the OpenAPI document - they all return `Results.Ok(...)`/
  `Results.Created(...)` (`IResult`), which the built-in OpenAPI generator can't infer a concrete
  type from without `.Produces<T>()` metadata. Added `.Produces<T>()` (and
  `.Produces(StatusCodes.Status204NoContent)` for deletes) to every endpoint in
  `AlertRulesEndpoints`, `ChannelsEndpoints`, `SubscriptionsEndpoints`, `NotificationsEndpoints`,
  and `EventsEndpoints`. This is a real improvement to Phase 08's OpenAPI output, not just a
  frontend concern - and it's what makes `openapi-typescript` generation actually useful (before
  this, only request body types were present in `components.schemas`).
- **Type generation**: `openapi-typescript` generates `frontend/src/api/schema.d.ts` from a
  running API instance (`npm run generate:types`, hitting `http://localhost:5062/openapi/v1.json`).
  `openapi-fetch` (`src/api/client.ts`) builds a fully typed client on top of that schema.
  `/health` is deliberately not part of this (health-check middleware isn't OpenAPI-annotated),
  so it's called with a plain `fetch` (`src/api/health.ts`).
- **Enum values are numbers, not names**: `EventCategory`/`Severity`/`NotificationStatus` all
  serialize as plain integers (System.Text.Json's default), and the OpenAPI document has no way
  to carry the C# member names alongside them. `src/api/enums.ts` hand-maintains label maps for
  display purposes; a comment there points back at the three Domain enum files that must be kept
  in sync if new members are ever added.
- **Error responses are hand-typed**: none of the endpoints declare error-response schemas (only
  success responses got `.Produces<T>()`, to avoid a large amount of repetitive
  `.ProducesProblem(...)` noise across every endpoint for this phase), so `ProblemDetails` is a
  small hand-written interface (`src/api/problemDetails.ts`) rather than a generated one. This is
  a reasonable trade-off since RFC 7807's shape is standard and stable, unlike the DTOs.
- **Dependency friction**: the Vite scaffold picked up TypeScript 6.0.x (npm's `latest` tag for
  the `typescript` package is already at a 7.x major in this environment's registry state), while
  `openapi-typescript`/`openapi-fetch`'s published peer ranges still say `^5.x`. Used
  `frontend/.npmrc` (`legacy-peer-deps=true`) rather than downgrading the whole toolchain to a
  stale TypeScript 5.x release; this is a known, recorded trade-off, not an oversight.
- **Auth flow**: `src/auth/ApiKeyContext.tsx` stores the key in `sessionStorage`;
  `src/auth/ApiKeyGate.tsx` blocks the routed app behind an entry screen
  (`src/auth/ApiKeyPrompt.tsx`) until a key is present; `src/api/client.ts`'s `onUnauthorized`
  callback (wired to `clearApiKey`) means any `401` naturally routes the user back to that screen.
- **Manually verified end-to-end** (not just `npm run build`): ran the API locally
  (`dotnet run --project src/NotifyMe.Api`) with a temporary `Admin:ApiKey` user-secret and the
  Vite dev server side by side, then drove the running app in a real browser: entered the key,
  landed on the Status page showing `/health reports: Healthy` (no CORS error), navigated to the
  Alert Rules placeholder via the nav, and confirmed "Forget API key" returns to the entry screen.
  Full backend `dotnet build`/`dotnet test` re-confirmed at 110/110 passing after the CORS/
  `Produces<T>()` changes.

## Status
Done (2026-09-22).

