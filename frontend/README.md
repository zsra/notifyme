# NotifyMe Admin frontend

A React/TypeScript admin panel for the NotifyMe Admin API, kept deliberately simple and focused
on technical substance over visual polish (see [ADR-0009](../ai/decisions/adr/0009-frontend-stack.md)).
Scaffolded with Vite + React + TypeScript.

## Stack

- **Vite + React + TypeScript**.
- **[openapi-typescript](https://openapi-ts.dev/)** generates `src/api/schema.d.ts` from the
  Admin API's own OpenAPI document, and **[openapi-fetch](https://openapi-ts.dev/openapi-fetch/)**
  (`src/api/client.ts`) provides a fully typed HTTP client built on that schema - request/response
  bodies are checked against the real API contract, not hand-maintained interfaces.
- **TanStack Query** for server state (fetching/caching/invalidation).
- **React Router** for navigation between the Alert Rules / Channels / Subscriptions /
  Notifications screens (`src/pages/`).
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
