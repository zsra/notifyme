# Phase 00 - Repo & AI-friendly scaffolding

## Goal
Set up the repository so it is easy for both humans and AI agents to navigate: a clear
separation between process artifacts (`ai/`) and design documentation (`docs/`), plus
root-level guidance files for AI coding agents.

## Depends on
Nothing. This is the first phase.

## Steps
- [x] Create `ai/`, `docs/`, `src/`, `tests/` folder skeletons.
- [x] Add `ai/README.md` explaining the `ai/` vs `docs/` split.
- [x] Add `ai/plan/00-plan-overview.md` with all phases listed and checkboxes.
- [x] Add one file per phase under `ai/plan/`.
- [x] Add `ai/decisions/adr/template.md` and the initial ADRs (Phase 01 covers writing their
      content in full; the folder and template exist as of this phase).
- [x] Add `ai/prompts/prompt-log.md` with the prompt history so far.
- [x] Add root `AGENTS.md` and `.github/copilot-instructions.md`.
- [x] Add `.gitignore` and `.editorconfig`.
- [x] Add root `README.md` pointing into `ai/` and `docs/`.

## Verification
- Folder tree matches the layout described in `ai/README.md` and `docs/` (see Phase 01).
- No `.NET` solution/code exists yet, only planning/decision/doc content, per explicit
  instruction to not implement yet.
- All new files render correctly as Markdown and links resolve to real files.

## Status
Done, as part of this scaffolding pass (2026-09-22).
