# Phase 15 - Frontend testing, CI, and docs

## Goal
Add automated tests for the frontend's actual logic, extend CI to cover it, and bring
documentation up to date, mirroring the rigor already applied to the backend (Phases 11-12).

## Depends on
Phase 14.

## Steps
- [x] Vitest + React Testing Library set up in `frontend/`.
- [x] Tests for the API client (request shaping, `X-Api-Key` header attachment, `ProblemDetails`
      error parsing) and for at least one non-trivial screen/hook (e.g. the Alert Rules list
      with filters, or the cache-invalidation behavior from Phase 14).
- [x] Extend `.github/workflows/ci.yml` with a frontend job (Node.js setup, `npm ci`, `npm run
      build`, `npm test`) alongside the existing backend job.
- [x] Update root `README.md`, `docs/runbook.md`, and `docs/architecture/overview.md` to
      describe the frontend (how to run it locally, how it fits the architecture diagram) now
      that it is real rather than deferred.
- [x] Final ADR pass for anything the frontend work surfaced that isn't already covered by
      ADR-0009.

## Verification
- `npm test` passes locally and in CI.
- CI is green with both the backend and frontend jobs on a fresh push.
- A fresh clone following the updated runbook can run both backend and frontend locally and use
  the UI end-to-end.

## Design notes

**Test setup**: `vitest` + `@testing-library/react` + `@testing-library/jest-dom` +
`@testing-library/user-event` + `jsdom`, configured in `vite.config.ts`'s `test` block
(`environment: 'jsdom'`, `setupFiles: ['./src/test/setup.ts']`, `globals: true`, `pool: 'forks'`).
`globals: true` is required, not just convenient: `@testing-library/react`'s automatic
between-test DOM cleanup only registers itself when a global `afterEach` exists, and without it
DOM nodes leaked across tests within the same file, causing spurious "found multiple elements"
failures. `pool: 'forks'` works around the default "threads" pool being unreliable in this
sandbox (see below).

**Test coverage added**:
- `src/api/problemDetails.test.ts` - parses a JSON `ProblemDetails` body, returns `null` for a
  non-JSON response, and returns `null` when a response claims `content-type: application/json`
  but isn't valid JSON.
- `src/api/apiError.test.ts` - `unwrap()` returns `data` on an ok response, throws `ApiError`
  carrying the `ProblemDetails.detail` message on a non-ok response, and falls back to a generic
  "Request failed with status N" message when the body isn't `ProblemDetails` JSON.
- `src/api/client.test.ts` - `createAdminApiClient()` attaches the `X-Api-Key` header to every
  outgoing `Request` (verified by mocking `globalThis.fetch` and inspecting the captured
  `Request` object, since `openapi-fetch` calls `fetch(request, ...)` with a real `Request`
  instance internally) and calls `onUnauthorized()` if and only if a response comes back 401.
- `src/pages/AlertRulesPage.test.tsx` - the one "non-trivial screen" test called for by this
  phase's steps. Mocks `useAdminApiClient` entirely (so no real backend/`ApiKeyContext` is
  needed) and covers: the list rendering from the mocked `GET`, the list re-querying with the
  right query params when the category filter changes, and the create form submitting a
  correctly-shaped `CreateAlertRuleRequest` body via `POST`.

**A real, reproducible Windows-only bug found and worked around**: running the test script via
`npm --prefix frontend run test` from the repo root (the pattern already used elsewhere in this
repo for async dev-server commands, to dodge a different, unrelated `cd`-in-async-mode bug)
reliably crashes every Vitest test file with `TypeError: Cannot read properties of undefined
(reading 'config')` - even a trivial `describe`/`it`/`expect` file with no application imports.
The exact same command run via a plain `cd frontend && npm test` (or `npm run test` from
inside `frontend/`) passes every time. The root cause wasn't pinned down further (time-boxed);
the workaround is documented in `frontend/README.md` and `docs/runbook.md` so nobody hits it
again, and CI's job uses `working-directory: frontend` (never `--prefix`) for exactly this
reason. Separately, the default "threads" pool was also observed to intermittently crash the
same way even with a plain `cd`; `pool: 'forks'` (both in `vite.config.ts` and belt-and-suspenders
as `--pool=forks` on the `test` npm script) resolved that.

**No new ADR**: ADR-0009 already anticipated and decided Vitest + React Testing Library as the
frontend testing stack, and already flagged Phase 15 as the point CI would be extended; nothing
this phase surfaced amounted to a new architectural decision (the Windows worker-pool issue above
is a tooling workaround, not a design choice, so it lives in README/runbook prose rather than an
ADR).

## Status
Done (2026-09-22).
