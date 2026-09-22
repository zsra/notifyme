# NotifyMe frontend

A React/TypeScript frontend for NotifyMe, kept deliberately simple and focused on technical
substance over visual polish (see [ADR-0009](../ai/decisions/adr/0009-frontend-stack.md)).
Scaffolded with Vite + React + TypeScript. Hosts two separate areas:

- **Admin panel** (`/`, `/alert-rules`, `/channels`, `/subscriptions`, `/notifications`): gated
  by the Admin API key (`src/auth/ApiKeyContext.tsx`), manages every row regardless of owner.
- **Self-service** (`/register`, `/login`, `/my`): gated by a per-user JWT bearer token
  (`src/auth/MyAuthContext.tsx`), lets anyone register their own account and manage only their
  own alert rules, channels, and subscriptions. See
  [ADR-0010](../ai/decisions/adr/0010-end-user-self-service.md).

These two auth mechanisms are entirely independent - each is stored under its own
`sessionStorage` key and sent as a different header (`X-Api-Key` vs. `Authorization: Bearer`),
and neither grants access to the other's endpoints.

## Stack

- **Vite + React + TypeScript**.
- **[openapi-typescript](https://openapi-ts.dev/)** generates `src/api/schema.d.ts` from the
  Admin API's own OpenAPI document, and **[openapi-fetch](https://openapi-ts.dev/openapi-fetch/)**
  (`src/api/client.ts`) provides a fully typed HTTP client built on that schema - request/response
  bodies are checked against the real API contract, not hand-maintained interfaces.
- **TanStack Query** for server state (fetching/caching/invalidation).
- **React Router** for navigation between the Alert Rules / Channels / Subscriptions /
  Notifications admin screens (`src/pages/`) and the self-service Login / Register / My Alerts
  screens.
- No CSS/UI framework and no form library, by design.

## Local development

Prerequisites: Node.js (current LTS), and the NotifyMe API running locally (see the root
[`docs/runbook.md`](../docs/runbook.md)) reachable at `http://localhost:5062` (its default
`http` launch profile). The API must have a `Cors:FrontendOrigin` matching this app's dev URL
(`http://localhost:5173` by default - already the config default in `appsettings.json`).

```
npm install
npm run dev
```

Open `http://localhost:5173`, enter the Admin API key configured on the backend (see
`dotnet user-secrets set "Admin:ApiKey" "<key>" --project ../src/NotifyMe.Api`), and you're in.
The key is kept in `sessionStorage` only, for the lifetime of the browser tab.

For the self-service area instead, go to `/register` to create an end-user account (or `/login`
if you already have one) - no Admin API key needed. The backend must have `Jwt:SigningKey`
configured (see `dotnet user-secrets set "Jwt:SigningKey" "<32+ byte random string>" --project
../src/NotifyMe.Api`) or every `/api/me/*` request will fail. The returned JWT is likewise kept
in `sessionStorage` only, under a separate key from the Admin API key.

### Regenerating API types

Whenever the Admin API's contract changes, regenerate the schema against a running instance:

```
npm run generate:types
```

This overwrites `src/api/schema.d.ts`. Never hand-edit that file.

### Other scripts

- `npm run build` - type-checks (`tsc -b`) and produces a production build.
- `npm run lint` - runs Oxlint.
- `npm run preview` - serves the production build locally.
- `npm test` - runs the Vitest suite once (`vitest run`). Run this from *inside* `frontend/`
  (e.g. `cd frontend && npm test`) rather than via `npm --prefix frontend run test` from the
  repo root - on Windows, `--prefix` reliably crashes Vitest's worker pool for reasons that
  aren't fully understood; a plain `cd` avoids it.

## Testing

Uses [Vitest](https://vitest.dev/) + [React Testing Library](https://testing-library.com/react)
+ jsdom. Test files sit next to the code they cover (`*.test.ts`/`*.test.tsx`), for example
`src/api/apiError.test.ts` and `src/pages/AlertRulesPage.test.tsx`. `useAdminApiClient` is mocked
in admin screen tests, and `useMyApiClient`/`useMyAuth`/`createPublicApiClient` are mocked in
self-service screen tests (e.g. `src/pages/MyAlertsPage.test.tsx`, `src/pages/LoginPage.test.tsx`),
so each screen exercises its own query/mutation wiring without a real backend.

`vite.config.ts`'s `test` block sets `environment: 'jsdom'`, `globals: true` (required so
`@testing-library/react`'s automatic per-test DOM cleanup can register itself via a global
`afterEach`), and `pool: 'forks'` (the default "threads" pool was unreliable in this sandbox).

