# Phase 12 - CI & repo polish

## Goal
Wrap up with continuous integration and a final documentation pass so the repository is in a
clean, reviewable state.

## Depends on
Phase 11 (test suite should be in its final shape).

## Steps
- [x] `.github/workflows/ci.yml`: restore, build, test on push and pull request.
- [x] Finalize root `README.md` with concrete run instructions once code exists.
- [x] Final pass over `ai/decisions/adr/` for completeness.
- [x] Tag/milestone commit marking the backend as feature-complete for this exercise.

## Verification
- GitHub Actions run is green on the default branch.
- A fresh clone + documented setup steps (docker compose up, dotnet ef database update, dotnet
  run) results in a working local instance without undocumented manual steps.

## Design notes

### CI workflow
Added `.github/workflows/ci.yml`, triggered on push and pull request to `main`. Single
`build-and-test` job on `ubuntu-latest`:
- `actions/setup-dotnet@v4` pinned to `10.0.x` (matches ADR-0001; no `global.json` in the repo,
  so the floating minor/patch is intentional).
- A `mailhog/mailhog:v1.0.1` service container (ports `1025`/`8025` mapped to the runner's
  `localhost`, matching `docker-compose.yml`) so `EmailNotificationChannelMailHogTests` passes in
  CI without any extra setup. Postgres-backed tests don't need a services entry: they spin up
  their own ephemeral container via Testcontainers (Phase 11), and `ubuntu-latest` runners have a
  Docker daemon available by default. WireMock-backed Slack tests need no Docker at all (in-
  process server).
- `dotnet restore` / `dotnet build --no-restore -c Release` / `dotnet test --no-build -c Release`.
  `TreatWarningsAsErrors` is already `true` repo-wide (`Directory.Build.props`), so `dotnet build`
  in CI catches the same warnings-as-errors locally-verified builds do.

### README
Rewrote the root `README.md`, which still described the Phase 02 scaffolding-only state (no
business logic, 7 projects, "no tests written yet"). Replaced with a "Quick start" section giving
the exact commands to go from a fresh clone to a running instance (`docker compose up -d`, user-
secrets, `dotnet ef database update`, `dotnet run`), a pointer to `dotnet test` and what each test
project needs, and an updated "Status" section describing the actual feature-complete state
(Phases 00-11) instead of the stale scaffolding-only description. Kept the existing `ai/`/`docs/`
folder map, since that structure hasn't changed.

### ADR review
Read all 8 existing ADRs (`0001`-`0008`) end to end. All are `Accepted`, dated, and still
accurately describe the implemented system (Testcontainers is already called out in ADR-0007;
the notification channel and event-source seams described in ADR-0004/ADR-0005 match the actual
`Infrastructure` folder layout). No gaps or contradictions found; no new ADR was needed for the
CI workflow itself, since it just executes the testing strategy ADR-0007 already decided rather
than making a new architectural choice.

### Tag/milestone commit
Left unchecked deliberately until the user explicitly confirmed it (committing/tagging is a git
action the agent does not take without explicit user instruction, see `AGENTS.md`/repo
convention). With that go-ahead, created an annotated tag `v1.0.0` on the current `main` HEAD
summarizing the feature-complete backend (Phases 00-12) and frontend (Phases 13-15). The tag was
created locally only; it was not pushed, since the user's go-ahead covered the local tag, not a
push (a separate, more shared-system-affecting action).

### NuGet Audit remediation (post-milestone CI failure)
After the milestone commit was pushed (by an external mechanism), a real GitHub Actions run
failed `dotnet restore` on both `src/NotifyMe.Api/NotifyMe.Api.csproj` and
`tests/NotifyMe.IntegrationTests/NotifyMe.IntegrationTests.csproj` with NU1902/NU1903 errors.
NuGet Audit (which queries the live GitHub Advisory database during restore) flagged newly
published advisories against transitive dependencies, and `TreatWarningsAsErrors=true`
(`Directory.Build.props`) turned those into hard restore failures. Locally this didn't reproduce
via a plain `dotnet restore` (the audit check appears to depend on live advisory-database
network access not available/exercised in this environment), so the fix was verified by directly
inspecting `dotnet list package --include-transitive` output and NuGet.org/GHSA advisory pages
rather than by reproducing the CI failure locally.

Root causes and fixes:
- `Microsoft.OpenApi` 2.0.0 (GHSA-v5pm-xwqc-g5wc, High): transitive via
  `Microsoft.AspNetCore.OpenApi` 10.0.10 in `NotifyMe.Api.csproj`, which depends on
  `Microsoft.OpenApi >= 2.0.0` with no upper bound. Fixed by adding a direct
  `<PackageReference Include="Microsoft.OpenApi" Version="2.12.2" />` (latest patched 2.x
  release; stayed on the 2.x line rather than jumping to 3.x to avoid an API-breaking change in
  `Microsoft.AspNetCore.OpenApi`'s generated OpenAPI code, which assumes the 2.x
  `IOpenApiMediaType.Example` shape).
- `OpenTelemetry.Api` / `OpenTelemetry.Exporter.OpenTelemetryProtocol` 1.14.0 (multiple GHSAs,
  Moderate) and `Scriban.Signed` 5.5.0 (multiple GHSAs, Moderate/High/Critical): both transitive
  via `WireMock.Net` 1.25.0 in `NotifyMe.IntegrationTests.csproj`. Fixed by upgrading
  `WireMock.Net` to `2.17.0`, which pulls patched `OpenTelemetry.*` 1.15.3 and `Scriban.Signed`
  7.2.5 automatically.
- The `WireMock.Net` 2.17.0 upgrade also pulls a transitive `Microsoft.OpenApi` 3.10.2 floor
  (via `WireMock.Net.OpenApiParser` -> `RamlToOpenApiConverter.SourceOnly`), which conflicts at
  compile time with the 2.x-based `Microsoft.AspNetCore.OpenApi` source-generated code that runs
  against the `NotifyMe.Api` project reference. Since the tests never exercise WireMock.Net's
  RAML/OpenAPI conversion feature, `NotifyMe.IntegrationTests.csproj` pins `Microsoft.OpenApi`
  back down to 2.12.2 (matching `NotifyMe.Api.csproj`) and suppresses the resulting NU1605
  ("package downgrade") warning for that project only.
- `WireMock.Net` 2.17.0's new nullable-annotated API surface (`ILogEntry.RequestMessage` is now
  nullable) surfaced one new `CS8602` nullable-warning-as-error in
  `SlackNotificationChannelWireMockTests.cs`; fixed with a null-forgiving operator since a
  completed request is guaranteed to have a non-null `RequestMessage`.

Verified with `dotnet build -c Release` (0 warnings, 0 errors) and `dotnet test -c Release`
(110/110 passing across all four test projects) after the changes.

## Status
Done (2026-09-22). All steps complete; `dotnet build`/`dotnet test` pass (110/110), and the repo
is tagged `v1.0.0` locally marking the feature-complete milestone (backend + frontend). NuGet
Audit vulnerability remediation (see above) applied after the initial milestone to keep CI green.
