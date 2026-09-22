---
description: Repository-specific conventions for GitHub Copilot in the NotifyMe project
applyTo: "**"
---

# Copilot instructions for NotifyMe

This repo has a full agent guidance file at [`AGENTS.md`](../AGENTS.md) - read it in full before
making non-trivial changes. Summary of the parts that matter most while coding:

- Check [`ai/plan/00-plan-overview.md`](../ai/plan/00-plan-overview.md) for current phase status
  before starting work; don't jump ahead of the plan without flagging it.
- Respect Clean Architecture dependency direction: `Domain` -> nothing, `Application` -> `Domain`
  only, `Infrastructure` -> implements `Domain`/`Application` interfaces, `Api` -> composes all.
- Keep the simulated event-source boundary (`IEventSource`) and the notification-channel
  abstraction (`INotificationChannel`) pluggable; see [`docs/architecture/`](../docs/architecture/).
- Never commit secrets (webhook URLs, SMTP credentials, API keys) anywhere in the repo, including
  in docs or examples. Use placeholders and `dotnet user-secrets`/environment variables.
- Log non-trivial decisions as a new ADR under [`ai/decisions/adr/`](../ai/decisions/adr/), and
  append prompt history to [`ai/prompts/prompt-log.md`](../ai/prompts/prompt-log.md).
- Prefer many small, topic-scoped files over one large file for docs/plans/decisions.
- Commit messages: `feat(phase-N): <summary>` / `docs(phase-N): <summary>`.
- A phase isn't done until `dotnet build` and `dotnet test` pass.
