# Brief compliance and gap report

Verification of the current repository state against the original brief,
[`task-04-feature-design-and-build.docx`](../../task-04-feature-design-and-build.docx), performed
by extracting the document's plain text and cross-checking it against the code, docs, ADRs, plan
files, and git history as they exist today (2026-09-22). This is a point-by-point comparison, not
a restatement of the plan.

## The brief, in full

> "We want users to be able to set up alerts so they get notified when something important
> happens in the world, like breaking news, market movements, natural disasters, that kind of
> thing. Should work for both email and Slack. Make it flexible enough that we can add more
> channels later. We need an admin view too."

Plus a process framing: no further specification is given on purpose, the submission is
evaluated on process (plan, decisions, course-corrections) more than on the polish of the code,
and a specific list of required submission artifacts is given (see the second table below).

## Part 1: Product requirements

| Brief says | Implementation says | Match |
|---|---|---|
| Users can set up alerts for "something important" happening in the world | `AlertRule` + `MatchCriteria` (Domain), CRUD via Admin API and the Alert Rules frontend page | Match |
| Categories: breaking news, market movements, natural disasters | `EventCategory` enum (`BreakingNews`, `MarketMovement`, `NaturalDisaster`), matched 1:1 | Match |
| Notify via email | `EmailNotificationChannel` (SMTP via MailKit, verified against MailHog) | Match |
| Notify via Slack | `SlackNotificationChannel` (Incoming Webhook POST, verified against WireMock) | Match |
| "Flexible enough that we can add more channels later" | `INotificationChannel` is a Domain-level interface; `ChannelConfig.ChannelType` is a string discriminator, not a closed enum; DI resolves `IEnumerable<INotificationChannel>` and builds the dispatch lookup from that. Adding a channel needs zero changes to Domain/Application (see [`docs/architecture/notification-channels.md`](../architecture/notification-channels.md)) | Match |
| "We need an admin view too" | `frontend/` (React/TypeScript): Alert Rules, Channels, Subscriptions, Notifications, and Status pages, with create/edit/delete where the API supports it and a manual event-trigger action | Match |
| Where events come from / how "important" is detected (left undefined by the brief) | Deliberately simulated (`SimulatedEventSource`), behind an `IEventSource` seam designed for a real source to be dropped in later with no Domain/Application changes; explicitly justified in ADR-0004 | Match, by design, and clearly documented as a scope cut rather than left silent |

No product requirement in the brief is unmet. The one arguable gap is that "get notified when
something important happens in the world" could be read as requiring a real external event feed;
the implementation deliberately simulates this and documents why (time-boxed exercise, no
external API was specified or agreed). That is a reasonable, well-justified interpretation, not
an oversight, but it's the single biggest literal deviation from the brief's wording and is worth
being upfront about if this is presented to someone who expected live data.

Two smaller, already self-identified product gaps are worth repeating here because they are real
and still open:

- `UpdateAlertRuleRequest` and `UpdateChannelConfigRequest` do not support changing `isEnabled`
  after creation; the edit forms only expose that field at creation time (noted in
  `ai/plan/00-plan-overview.md`, Phase 14 status, not yet fixed).
- `EventCategory` is a closed enum (by design, contrasted explicitly with the open-ended channel
  registry), so adding a fourth category is a conscious Domain change, not a plug-in. The brief's
  "that kind of thing" wording leaves room to argue categories should be as open-ended as
  channels; the current design takes the position that they shouldn't be, and says so in code
  comments, but this is a judgment call rather than something the brief settles.

## Part 2: Process and submission requirements

The brief separately specifies what must be shared via the GitHub repository: commits per major
milestone with meaningful messages, prompt history, plans/decision logs/working artifacts, and
the deliverables themselves.

| Brief requires | What actually exists | Match |
|---|---|---|
| "commits for each major milestone, with meaningful messages" | 22 commits, one roughly per phase; messages are descriptive prose ("Implement Domain Layer: Complete entities, value objects, and interfaces") | Match |
| "your plans, working artifacts, and evidence of how you worked" | `ai/plan/` (16 phase files + overview) and `ai/decisions/adr/` (9 ADRs) are thorough, specific, and consistently updated | Match, and the strongest part of the submission |
| "...design docs, decision logs, prompt drafts, intermediate outputs, notes, scratch files, screenshots, etc." | Decision logs: present and detailed. Design docs: present (`docs/architecture/`, `docs/api/`). Prompt drafts, intermediate outputs, notes, scratch files, screenshots: none found anywhere in the repo (`**/*.png`, `**/*.jpg` searches returned nothing) | Partial. The plan files repeatedly assert things were "verified end-to-end locally" or "verified through the actual UI," but there is no artifact in the repo (screenshot, exported log, recorded output) backing any of those specific verification claims - they exist only as prose assertions |
| "your deliverables, as defined by your own plan" | Backend (5 projects) and frontend both feature-complete per the plan; `dotnet build`/`dotnet test` and `npm run build`/`npm test` all pass; CI runs both on push/PR | Match |

## Summary

Product-wise, the brief is fully satisfied: alerts across the three named categories, both
channels working end-to-end, a documented and genuinely open-ended channel extension point, and
an admin view. The event-source simulation is the one deliberate, well-documented deviation from
a literal reading.

Process-wise, the plan and decision log are unusually thorough and are the strongest part of the
submission.
