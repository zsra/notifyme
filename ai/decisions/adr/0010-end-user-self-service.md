# ADR-0010: End-user self-service (accounts, ownership, JWT auth)

- **Status**: Accepted
- **Date**: 2026-09-22

## Context
The brief says: "We want users to be able to set up alerts... We need an admin view too." Through
Phase 15 this was read as one thing: a single admin panel, gated by one shared Admin API key,
used by whoever configures the system. A later review of that interpretation
(`docs/verification/BriefComplianceReport.md`) flagged it as a real, named gap: the brief reads
more naturally as *two* things, a self-service experience for end users to manage their own
alerts, and a separate admin view for operating the system as a whole. There is currently no
concept of a "user" anywhere in the domain model, no accounts, no authentication beyond the
single shared Admin API key, and no way for anyone other than the admin-key holder to create or
see an alert rule.

This ADR decides how to add that missing self-service layer without disturbing the existing,
already-shipped Admin API/frontend, and without a full rewrite of the data model or auth story.

## Decision
1. **New `User` domain entity** (`Domain/Users/User.cs`): id, normalized email, password hash,
   creation timestamp. No roles/permissions system: every registered user has identical
   capabilities (manage their own alert rules/channels/subscriptions). Multi-role admin-vs-user
   distinction stays exactly as it already was, the Admin API key.
2. **Ownership by nullable `OwnerUserId`, not a parallel data model.** `AlertRule`, `ChannelConfig`,
   and `Subscription` each gain a nullable `OwnerUserId` column. `null` means "admin/global"
   (every row created through today's Admin API keeps behaving exactly as before, invisible
   change). A non-null value means "owned by this end user." The existing Admin API and
   Admin use cases are untouched in behavior; a new optional `ownerUserId` parameter is added to
   the existing Create/List/Get/Update/Delete use cases (defaulting to `null`) so the same use
   case logic serves both the Admin API (passes nothing, sees/edits everything, exactly as
   before) and the new self-service API (passes the caller's id, is restricted to their own
   rows). This was chosen over duplicating a parallel set of "user" use cases because the
   underlying business logic (validation, matching-criteria construction, persistence) is
   identical either way; only the visibility/ownership check differs.
3. **JWT bearer auth for end users, kept entirely separate from the Admin API key.** Register
   (`POST /api/auth/register`) and log in (`POST /api/auth/login`) are public endpoints; login
   returns a short-lived signed JWT carrying the user's id. A new `/api/me/*` endpoint group
   requires that JWT (ASP.NET Core's built-in JWT bearer authentication/authorization, not the
   custom `ApiKeyEndpointFilter`). `/api/admin/*` is unchanged. The two auth mechanisms coexist
   without interacting: an end-user JWT does not grant access to `/api/admin/*`, and the Admin
   API key does not grant access to `/api/me/*`.
4. **Password hashing via the .NET BCL's built-in PBKDF2** (`Rfc2898DeriveBytes.Pbkdf2`,
   SHA-256, 100,000 iterations, a random 16-byte salt per user, stored as
   `{iterations}.{base64 salt}.{base64 hash}`), not a hand-rolled scheme and not a new
   third-party dependency. `System.Security.Cryptography` already ships in the BCL, so this adds
   zero new NuGet packages to `Infrastructure`.
5. **JWT issuance/validation lives in `Api`, not `Infrastructure`.** `Api` is already a Web SDK
   project with a framework reference to `Microsoft.AspNetCore.App`, and needs the
   `Microsoft.AspNetCore.Authentication.JwtBearer` package for the authentication middleware
   regardless; implementing `IJwtTokenGenerator` (declared in `Application/Abstractions`) there
   too avoids pulling `System.IdentityModel.Tokens.Jwt`/`Microsoft.IdentityModel.Tokens` into the
   plain class library `Infrastructure` project just for token minting. This is consistent with
   `AGENTS.md`: `Api` is "the only project allowed to reference all three [layers]... composes
   everything via DI," and implementing one more interface there doesn't violate the
   `Domain`/`Application`/`Infrastructure` dependency direction since `Application` only depends
   on the `IJwtTokenGenerator` abstraction, never on the concrete JWT library.
6. **No "my notifications" endpoint for now.** Notification history is a read-only reporting
   concern already fully served by the Admin API; the part of the brief actually at stake here is
   "users can set up alerts," i.e. create/manage alert rules, channels, and subscriptions. Adding
   user-scoped notification history is a natural, low-risk follow-up (the ownership chain already
   exists via `Subscription.OwnerUserId`) but is left out of this phase to keep it bounded.

## Alternatives considered
- **Full ASP.NET Core Identity (with its own EF store/migrations)** - more batteries included
  (password reset flows, lockout, etc.) but a much heavier data model and migration footprint for
  a single-role, no-lockout, no-email-verification use case. Rejected as disproportionate to what
  the brief actually asks for.
- **A parallel `Tenant`/multi-tenant partition key on every table, with a separate "user API"
  surface duplicating every Admin use case** - cleaner conceptual separation, but doubles the
  amount of code to maintain for logic that's otherwise identical, and the brief doesn't call for
  multiple isolated tenants, just "users" and "an admin view." Rejected in favor of the additive
  `OwnerUserId` + optional-parameter approach.
- **Cookie-based session auth for end users** - would need CSRF protection and server-side session
  storage; JWT bearer keeps the new surface stateless and consistent with the existing frontend's
  header-based auth pattern (it already sends `X-Api-Key` as a header for the admin panel; the
  self-service frontend sends `Authorization: Bearer <token>` the same way).

## Consequences
Every existing Admin API endpoint, use case, and test keeps behaving exactly as it did (the new
`ownerUserId` parameter defaults to `null` everywhere it's added, an admin-created row is
indistinguishable from a Phase 08-era one). The new `/api/me/*` surface and the `User` entity are
purely additive. Known limitations, left as explicit follow-ups rather than silent gaps: no
password reset/email verification flow, no per-user rate limiting, no "my notifications" history
endpoint yet, and a user cannot currently be promoted to admin (the Admin API key remains the only
admin credential).
