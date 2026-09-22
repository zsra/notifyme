# ai/

This folder holds the **process artifacts** for this project: the plan, the decision log, and
the prompt history. It is intentionally kept separate from [`docs/`](../docs/), which holds the
**actual design documentation** for the system being built.

Rule of thumb: if it's about *how and why we worked*, it goes here. If it's about *what the
system is and how it's shaped*, it goes in `docs/`.

## Subfolders

- [`plan/`](plan/) - the phased implementation plan. `00-plan-overview.md` is the index and
  status tracker; each phase has its own small file with goal, steps, and verification criteria.
- [`decisions/adr/`](decisions/adr/) - Architecture/process Decision Records. One file per
  decision, using `template.md`. Includes both up-front decisions and any made mid-build.
- [`prompts/`](prompts/) - `prompt-log.md`, a chronological, append-only record of the prompts
  used to direct AI during this project, and what was accepted, rejected, or corrected as a
  result. This is a required deliverable for the exercise this project comes from, not optional.

## Ground rules

- Keep files small and topic-scoped. Don't let any single file become a dumping ground.
- Update `plan/00-plan-overview.md` checkboxes as phases complete; don't let it go stale.
- Every new non-trivial decision gets its own ADR, even if it's a small course-correction.
- Every significant prompt/interaction gets appended to `prompts/prompt-log.md`, including the
  ones where AI output was rejected or corrected, not just the successful ones.
