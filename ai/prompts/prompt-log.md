# Prompt log

Chronological, append-only record of prompts used to direct AI during this project, and what
happened as a result, what was accepted, what was rejected or corrected. This is a required
deliverable for the exercise this project comes from.

Format per entry: date, the prompt (verbatim or close paraphrase), and an outcome summary
including any corrections/rejections.

---

## 2026-09-22 - Entry 1: Initial brief review

**Prompt**: "Check my docx file"

**Outcome**: Located `task-04-feature-design-and-build.docx` in the repo root (only file
present besides `.git/`). Extracted its contents (it's a zipped OOXML file, not directly
readable as text, so extracted `word/document.xml` and parsed the paragraph text out of it) and
summarized the brief and its evaluation criteria back to the user.

## 2026-09-22 - Entry 2: Backend-first implementation plan request

**Prompt**: "Create an implementation plan using .NET backend first later we will discuss the
frontend. Since the task at least admit we can use AI, want you to setup my repository as AI
friendly as possible with the best industry standard, so my expectation is that we have a folder
that contains information on decisions and of course the implementation plan, goes small steps
so do not skipping smaller part. Separate AI plan with actual design documentation. The plan
should look like that someone who worked as a Lead SWE work on designs and abstraction, lets say
be interactive planning."

**Outcome**: Rather than guessing at ambiguous technical choices, asked 8 clarifying questions
covering: .NET version, architecture style, database choice, event-source strategy (the most
ambiguous part of the brief), notification channel implementation, admin API scope/auth, testing
depth, and whether to add root-level AI agent guidance files. User answered all 8; notably chose
a fully simulated event source over the recommended hybrid (real API + mock), with an explicit
requirement that it be architecturally separated from domain logic as a visible seam. Produced a
13-phase implementation plan (Phase 00 - repo scaffolding through Phase 12 - CI/polish, frontend
explicitly excluded) and saved it to session memory pending review. No repository files were
created in this step, per operating in a plan-only mode at the time.

## 2026-09-22 - Entry 3: Write the plan into the repository

**Prompt**: "Okay let's write down in these stuff in the repo. Avoid one large file try to split
them and do not implement yet"

**Outcome**: Converted the session-memory plan into actual repository files, split into many
small topic-scoped files rather than one large document: root `AGENTS.md`,
`.github/copilot-instructions.md`, `.gitignore`, `.editorconfig`, `README.md`; `ai/README.md`,
`ai/plan/00-plan-overview.md` plus one file per phase (13 total); `ai/decisions/adr/template.md`
plus ADR-0001 through ADR-0008 covering every decision made in Entry 2; this prompt log. Explicit
instruction to not implement yet was honored: no `.NET` solution or source code was created, only
planning/decision/documentation content (Phase 00 and Phase 01 of the plan).
