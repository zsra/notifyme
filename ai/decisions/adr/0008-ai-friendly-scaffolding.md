# ADR-0008: AI-friendly repository scaffolding

- **Status**: Accepted
- **Date**: 2026-09-22

## Context
The exercise this project comes from is evaluated primarily on process: how AI was directed,
what decisions were made and why, and how outputs were validated or corrected. The repository
itself needs to make that process visible and needs to be easy for any AI coding agent (this one
or another) to pick up with full context.

## Decision
Add a root-level `AGENTS.md` (general agent guidance) and `.github/copilot-instructions.md`
(Copilot-specific, points back to `AGENTS.md`), in addition to two strictly separated folders:
`ai/` for process artifacts (plan, decision log) and `docs/` for actual design documentation
(architecture, data model, API contracts). Keep individual files small and topic-scoped rather
than a small number of large documents.

## Alternatives considered
- `ai/` and `docs/` only, no root agent guidance file - smaller footprint, but leaves any future
  AI agent without a single, discoverable entry point for repo conventions.
- A single combined "planning" folder mixing process and design content - simpler folder count,
  but blurs the distinction the exercise's own submission requirements draw between "your plans/
  decision logs" and "your deliverables" (the design docs and code).

## Consequences
More root-level files and folder ceremony than a minimal repo would have. In exchange, the
process (what's required for evaluation) and the product design (what's being built) are never
tangled together, and any AI agent opening this repo cold has an immediate, explicit map of where
to look and what conventions to follow.

## Amendment (2026-09-22)
Initially this ADR also introduced a hand-maintained `ai/prompts/prompt-log.md`, manually
appended after each significant prompt. Dropped that in favor of relying on git commit history
(meaningful, phase-referencing commit messages) plus the coding tool's own session/conversation
records as the source of prompt history, since a manually-maintained log was on track to become
an ever-growing, easily-stale file that duplicated information already captured elsewhere. If
the project's evaluator specifically needs an exported prompt history artifact, generate it from
the tool's session records at submission time rather than maintaining it continuously by hand.
