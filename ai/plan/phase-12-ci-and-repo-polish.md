# Phase 12 - CI & repo polish

## Goal
Wrap up with continuous integration and a final documentation pass so the repository is in a
clean, reviewable state.

## Depends on
Phase 11 (test suite should be in its final shape).

## Steps
- [ ] `.github/workflows/ci.yml`: restore, build, test on push and pull request.
- [ ] Finalize root `README.md` with concrete run instructions once code exists.
- [ ] Final pass over `ai/decisions/adr/` and `ai/prompts/prompt-log.md` for completeness.
- [ ] Tag/milestone commit marking the backend as feature-complete for this exercise.

## Verification
- GitHub Actions run is green on the default branch.
- A fresh clone + documented setup steps (docker compose up, dotnet ef database update, dotnet
  run) results in a working local instance without undocumented manual steps.

## Status
Not started.
