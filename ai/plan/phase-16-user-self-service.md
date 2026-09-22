# Phase 16 - End-user self-service (accounts + user-scoped alerts)

## Goal

Close the gap identified against the original brief (see
`docs/verification/BriefComplianceReport.md` and ADR-0010): give end users a way to register, log
in, and manage their own alert rules, channels, and subscriptions, without the Admin API key, as a
capability separate from (and additive to) the existing admin panel.

## Depends on

Phases 00-15 (all done). Builds on the existing `AlertRule`/`ChannelConfig`/`Subscription`
entities and Admin API use cases rather than replacing them.

## Steps

1. **Domain**: add a `User` entity (`Domain/Users/User.cs`); add a nullable `OwnerUserId` to
   `AlertRule`, `ChannelConfig`, `Subscription` (see ADR-0010 for why nullable-column ownership
   rather than a parallel data model).
2. **Application**:
   - `Abstractions/Repositories/IUserRepository`, `Abstractions/IPasswordHasher`,
     `Abstractions/IJwtTokenGenerator`.
   - `Users/` module: `RegisterUserRequest`(+validator), `RegisterUserUseCase`,
     `LoginUserRequest`(+validator), `LoginUserUseCase`, `UserDto`, `AuthResultDto`.
   - Add an optional `Guid? ownerUserId = null` parameter to the existing Create/List/Get/Update/
     Delete use cases for `AlertRule`/`ChannelConfig` and to `ManageSubscriptionUseCase`/
     `ListSubscriptionsUseCase`, enforcing filtering (list) or ownership (get/update/delete) only
     when a caller id is supplied. Existing Admin call sites are unaffected (default `null`).
3. **Infrastructure**: `UserConfiguration`/`DbSet<User>`, `UserRepository`; `OwnerUserId` column
   mapping on the three existing configurations; a new EF Core migration;
   `Security/PasswordHasher` (BCL PBKDF2, see ADR-0010).
4. **Api**:
   - JWT bearer authentication wired in `Program.cs` (`Jwt:Issuer`/`Jwt:Audience`/
     `Jwt:SigningKey` config, the signing key from user secrets/environment, never committed).
   - `Api/Authentication/JwtTokenGenerator.cs` (implements `IJwtTokenGenerator`).
   - `Endpoints/AuthEndpoints.cs` (`POST /api/auth/register`, `POST /api/auth/login`, public).
   - `Endpoints/MyAlertRulesEndpoints.cs`, `MyChannelsEndpoints.cs`, `MySubscriptionsEndpoints.cs`
     under a `/api/me` group requiring the JWT bearer scheme, each resolving the caller's user id
     from the token's claims and passing it through as `ownerUserId`.
5. **Tests**: unit tests for `RegisterUserUseCase`/`LoginUserUseCase` and `PasswordHasher`
   (Domain/Application test projects); integration tests covering register -> login -> create an
   alert rule/channel/subscription under `/api/me`, plus explicit ownership-isolation cases (a
   second user, and the Admin API, cannot see or modify another user's rows).
6. **Frontend**: `LoginPage`/`RegisterPage` (public routes) and a single consolidated
   `MyAlertsPage` (self-service: create/list/delete the caller's own alert rules, channels, and
   subscriptions), using a bearer token stored the same way the admin panel stores its API key
   (`sessionStorage`, tab lifetime only). Kept as one page rather than mirroring all four admin
   screens, to stay proportionate to what a first self-service cut needs.
7. **Docs**: update `docs/architecture/overview.md`, `docs/architecture/data-model.md`,
   `docs/runbook.md`, root `README.md`, and `frontend/README.md` to describe the self-service
   surface; update `ai/plan/00-plan-overview.md`.

## Verification criteria

- `dotnet build` and `dotnet test` pass (new unit + integration tests included).
- A fresh user can register, log in, create an alert rule/channel/subscription via `/api/me/*`,
  and see only their own rows, no admin-created or other-user rows leak into their lists, and the
  admin panel/API still sees and can manage everything as before.
- `npm run build` and `npm test` pass for the frontend; the new pages are reachable and functional
  against a running backend.

## Status

Done (2026-09-22).
