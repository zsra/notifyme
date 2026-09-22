# ADR-0009: Frontend stack: Vite + React + TypeScript, generated API types, minimal styling

- **Status**: Accepted
- **Date**: 2026-09-22

## Context
The brief asks for "an admin view"; the backend (Admin API, Phase 08) has been stable since
Phase 08 and the plan explicitly deferred frontend scoping until then (see
`ai/plan/00-plan-overview.md`). The user has now asked to plan the frontend, specifying React,
and explicitly asking to keep it simple and prioritize technical substance over visual polish.
This ADR covers the tooling, data-fetching/state, styling, and API-key-handling decisions needed
before writing any frontend code.

## Decision
- **Vite + React + TypeScript**, scaffolded in a new top-level `frontend/` folder (sibling to
  `src/`/`tests/`), using npm.
- **Generate TypeScript types for the Admin API's DTOs from the API's own OpenAPI document**
  (e.g. `openapi-typescript`) rather than hand-maintaining a parallel set of interfaces, so the
  frontend's view of the contract cannot silently drift from what `NotifyMe.Api` actually serves
  (see `docs/api/admin-api.md`). Generated output is checked in but regenerated via a documented
  script, never hand-edited.
- **TanStack Query (React Query)** for server state (fetching, caching, invalidation after
  mutations), since almost everything this app renders is a CRUD-over-REST view of the Admin
  API; this avoids hand-rolling cache invalidation with `useEffect`/`useState`.
- **React Router** for navigation between the resource screens (Alert Rules, Channels,
  Subscriptions, Notifications) and a landing/status page. No SSR framework (Next.js): there is
  no SEO/SSR requirement, only a local admin tool.
- **No CSS/UI framework** (no Tailwind, MUI, etc.) and **no form library** (no react-hook-form):
  a single small stylesheet for legibility, and plain controlled inputs for the small CRUD forms
  involved, consistent with the explicit "technical, not visual" priority and keeping dependency
  surface area small.
- **API key handling**: the Admin API key (see ADR-0006) is entered once through a simple entry
  screen and stored in `sessionStorage` (not `localStorage`), attached as an `X-Api-Key` header
  by a thin fetch wrapper shared by all Admin API calls; cleared when the tab closes. This
  matches ADR-0006's own framing of the key as a dev-grade placeholder, not a production auth
  story - `sessionStorage` is a pragmatic, low-effort improvement over `localStorage`, not a
  claim of real security hardening.
- **Backend change required**: `NotifyMe.Api` needs a CORS policy allowing the frontend's local
  dev origin (configurable, not hardcoded), since the Vite dev server and the API run on
  different ports locally.
- **Testing**: Vitest + React Testing Library for the pieces with actual logic (the API client,
  key hooks/forms). No end-to-end browser testing (e.g. Playwright) for this exercise, to keep
  scope proportional to "keep it simple."

## Alternatives considered
- Next.js - rejected: no SSR/SEO/routing complexity that would justify it for a local admin
  tool calling one existing REST API.
- Create React App - rejected: deprecated/unmaintained.
- Redux or Zustand for state management - rejected in favor of TanStack Query, since nearly all
  state here is server state (list/detail data from the API), not client-only UI state.
- Hand-maintained TypeScript interfaces mirroring the DTOs - rejected in favor of generating
  types from the OpenAPI document, to make contract drift a build-time fact instead of something
  that has to be remembered.
- Tailwind/MUI or another UI kit - rejected: adds exactly the visual/styling surface area the
  user asked to de-prioritize.
- `localStorage` for the API key - rejected in favor of `sessionStorage`; still not a real
  secret store, but reduces the persistence window somewhat.
- Playwright/e2e tests - deferred as out of scope for now; can be added later if warranted.

## Consequences
- Requires Node.js (current LTS) locally and, once Phase 15 extends CI, in the CI runner
  alongside the existing .NET SDK requirement.
- `NotifyMe.Api` gains a small but real change (a CORS policy) purely to support local frontend
  development; this must be configuration-driven so it can be tightened per environment.
- Because types are generated from the live OpenAPI document, frontend development requires a
  running instance of the API (or a checked-in snapshot of its OpenAPI JSON) to regenerate types
  against; acceptable given the repo is already built around `docker compose up` + `dotnet run`
  for local dev.
- The UI is deliberately minimal/unstyled. If a polished UI is wanted later, that is a distinct,
  separate piece of work and this ADR would need revisiting; that is an intentional, temporary
  scope limitation, not an oversight.
