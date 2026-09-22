# Phase 15 - Frontend testing, CI, and docs

## Goal
Add automated tests for the frontend's actual logic, extend CI to cover it, and bring
documentation up to date, mirroring the rigor already applied to the backend (Phases 11-12).

## Depends on
Phase 14.

## Steps
- [ ] Vitest + React Testing Library set up in `frontend/`.
- [ ] Tests for the API client (request shaping, `X-Api-Key` header attachment, `ProblemDetails`
      error parsing) and for at least one non-trivial screen/hook (e.g. the Alert Rules list
      with filters, or the cache-invalidation behavior from Phase 14).
- [ ] Extend `.github/workflows/ci.yml` with a frontend job (Node.js setup, `npm ci`, `npm run
      build`, `npm test`) alongside the existing backend job.
- [ ] Update root `README.md`, `docs/runbook.md`, and `docs/architecture/overview.md` to
      describe the frontend (how to run it locally, how it fits the architecture diagram) now
      that it is real rather than deferred.
- [ ] Final ADR pass for anything the frontend work surfaced that isn't already covered by
      ADR-0009.

## Verification
- `npm test` passes locally and in CI.
- CI is green with both the backend and frontend jobs on a fresh push.
- A fresh clone following the updated runbook can run both backend and frontend locally and use
  the UI end-to-end.

## Status
Not started.
