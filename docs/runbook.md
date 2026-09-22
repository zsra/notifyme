# Runbook (draft, placeholder)

Status: draft placeholder written in Phase 01, before any code exists. This will be rewritten
with concrete, verified steps once Phase 02 (solution scaffolding) and later phases land.

## Expected local setup, once implemented

1. `docker compose up -d` - starts PostgreSQL and MailHog.
2. `dotnet user-secrets set "Notifications:Slack:WebhookUrl" "<your webhook url>"` (and any
   other secrets) against `src/NotifyMe.Api`.
3. `dotnet ef database update --project src/NotifyMe.Infrastructure --startup-project src/NotifyMe.Api`
   - applies migrations.
4. `dotnet run --project src/NotifyMe.Api` - starts the API.
5. Open Swagger at the API's `/swagger` endpoint to explore the Admin API.
6. Open MailHog's web UI (default `http://localhost:8025`) to see delivered emails.
7. Use `POST /api/admin/events/trigger-simulated` (with the `X-Api-Key` header) to force an event
   through the pipeline on demand instead of waiting for the background worker's polling
   interval.

## Running tests

- `dotnet test` at the repo root runs unit tests.
- Integration tests require Docker running locally (Testcontainers spins up PostgreSQL; WireMock.Net
  and MailHog checks are part of the same suite).

## Known gaps as of this draft

No code exists yet. This document exists so the eventual "how do I run this" answer has a home
from the start, rather than being written as an afterthought.
