# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

InnoClinic is a clinic management system: .NET 10 microservices in `Backend/` and a React 19 + Vite + MUI SPA in `Frontend/`. Auth is Auth0 (JWT bearer on APIs, `@auth0/auth0-react` on the SPA).

## Commands

### Backend (run from `Backend/`)

Each service has its own solution: `InnoClinic.Profiles.slnx`, `InnoClinic.Appointments.slnx`, `InnoClinic.Notifications.slnx`.

```bash
dotnet restore InnoClinic.Profiles.slnx          # needs NUGET_USER / NUGET_TOKEN env vars (GitHub Packages feed)
dotnet build InnoClinic.Profiles.slnx
dotnet test --solution InnoClinic.Profiles.slnx  # all tests; Microsoft.Testing.Platform runner (global.json), xUnit v3
dotnet test --project Profiles/Profiles.UnitTests
dotnet test --project Profiles/Profiles.UnitTests --no-build --filter-class "*MedicalStaffServiceTests"
dotnet test --project Profiles/Profiles.UnitTests --no-build --filter-method "*CreateAsync_WhenLicenseNumberDuplicate_ReturnsConflictError"
docker compose up -d                              # full stack; copy .env.example to .env first
```

- Because of MTP, `dotnet test` needs `--solution`/`--project` and uses xUnit v3 filters (`--filter-class`, `--filter-method`, `--filter-namespace`, wildcards allowed), not VSTest's `--filter "FullyQualifiedName~..."`.
- Only Profiles has tests (`Profiles.UnitTests`, `Profiles.IntegrationTests`). Integration tests use Testcontainers (PostgreSQL) + Respawn, so Docker Desktop must be running, or every integration test fails with a "failed to connect to the docker API" error. They go through `ProfilesApiFactory` with a `TestAuthHandler` and `FakeAuthManagementService` in place of Auth0.
- EF Core migrations are applied automatically on startup in Development (`ApplyMigrationsAsync`). Migrations live in `Profiles.DAL/Migrations` and `Appointments.Infrastructure/Data/Migrations`; use the `ef-migration` skill for the exact commands.
- Package versions are managed centrally in `Directory.Packages.props`; csproj `PackageReference`s have no versions.
- Local compose ports: Profiles API 5000/5001, Appointments API 6000/6001, Postgres 5433, Redis 6379, RabbitMQ 5672 (UI 15672), Mailpit 8025. In Development, OpenAPI and Scalar are mapped anonymously.

### Frontend (run from `Frontend/`)

```bash
npm run dev            # Vite on port 5500 (strictPort; the backend CORS and Auth0 config expect this)
npm run build          # tsc -b && vite build
npm run lint           # ESLint, including FSD layer boundaries (eslint-plugin-boundaries)
npm run lint:fsd       # Steiger FSD structure checks
npm run format:check   # Prettier, LF line endings
```

There are no frontend tests. CI runs `audit`, `format:check`, `lint`, `lint:fsd`, `build`. A husky pre-commit hook (installed from the repo root into `Frontend/.husky`) runs lint-staged plus `lint:fsd`.

## Shared libraries are NuGet packages

`Backend/Shared/*` (`InnoClinic.Core`, `InnoClinic.AspNetCore`, `InnoClinic.Contracts.Grpc`, `InnoClinic.Messaging`) appear in the service solutions, but the services consume them as **PackageReferences** from the GitHub Packages feed (`nuget.config`, mapped to `InnoClinic.*`), not as project references. A change in `Shared/` has no effect on a service until the package `<Version>` in the shared csproj is bumped, the package is published, and the version in `Directory.Packages.props` is updated. To build against unpublished shared changes, use the `shared-package-local` skill (a temporary local feed under the git-ignored `Backend/obj/`). Don't pollute the global NuGet cache, and don't publish to the feed without asking.

## Architecture

### Services

- **Profiles** (`Profiles.API` → `BLL` → `DAL` → `Domain`): classic layered design. Minimal-API endpoint groups in `Profiles.API/Endpoints`, FluentValidation validators, Mapster mapping (`Mapping/Registers`, scanned at startup), services in BLL, EF Core repositories in DAL. Owns patients, medical staff, and specializations, and creates Auth0 users and roles through the Auth0 Management API (`Auth0.ManagementApi`).
- **Appointments** (`Appointments.API` + `Domain` + `Infrastructure`): vertical slices (new ones via the `appointments-feature` skill). Each `Features/<UseCase>/` folder holds Command/Query, Handler (MediatR), Validator (run by `ValidationBehavior`), and an `IEndpoint` class discovered by `AddEndpoints(assembly)`. Writes use EF Core (`AppointmentsDbContext`); read-side queries use Dapper via `ISqlConnectionFactory`. Redis is used for caching.
- **Appointments.Functions**: an Azure Functions (isolated worker) timer job, `AppointmentReminderFunction`, that reuses `Appointments.Infrastructure` and calls Profiles over gRPC to enqueue reminder emails.
- **Notifications.Worker**: a MassTransit/RabbitMQ consumer that renders Scriban HTML templates (`Templates/`) and sends mail via MailKit (Mailpit locally).

### Inter-service communication

- **gRPC** (contracts in `Shared/InnoClinic.Contracts.Grpc/Protos`): Profiles implements `PatientService.GetPatient` and `StaffScheduleSyncService.GetStaffProfile`, while Appointments implements `StaffScheduleSyncService.SyncStaffProfile`. The two APIs each implement different RPCs of the same service, so a client must point at the right host for the RPC it calls. gRPC endpoints require an internal M2M scope (`RequireInternalScope(InternalScopes.X)`) obtained with the separate `Auth0:Services` client credentials.
- **Staff sync outbox**: Profiles writes `SyncStaffProfileRequest` rows to its own outbox (`IOutboxRepository`), and `OutboxProcessorJob` pushes them to Appointments over gRPC.
- **Notification outbox**: both APIs write notification messages via `INotificationProducer` into a Postgres outbox. A `NotificationWorker` (subclass of `PostgresOutboxProcessor` from `InnoClinic.Messaging`) publishes them to RabbitMQ, and Notifications.Worker consumes them. Message contracts are in `InnoClinic.Messaging/Contracts`.

### Authorization model

This lives in `InnoClinic.AspNetCore/Authorization` and `InnoClinic.Core/Authorization`.

- Endpoints declare coarse permissions with `.RequirePermission(ClinicPermissions.X.Y)` (Auth0 RBAC permissions in the token).
- Ownership and row-level checks go through `IAccessResolver` + per-resource `IAccessPolicy<T>` implementations (e.g. `AppointmentAccessPolicy`, `Profiles.BLL/Authorization/*AccessPolicy`), using `ICurrentUser`.
- There are four roles (patient, doctor, administrator, receptionist), one per user. The role is stored both in Auth0 RBAC and in `app_metadata.role`, and provisioning code must keep the two in sync.

### Frontend

The frontend follows Feature-Sliced Design: `app → pages → widgets → features → entities → shared`. Imports may only go downward. ESLint `boundaries/dependencies` and Steiger enforce this. Use the path aliases `@app`, `@pages`, `@widgets`, `@features`, `@entities`, `@shared`, and import slices through their public `index.ts`. The API client is axios in `shared/api`, and env is read from `VITE_*` variables (see `.env.example`).

## Environments

The app runs in two places with different wiring. Keep both working when you change config, ports, or hosting code.

**Local: `Backend/docker-compose.yml`.** Postgres, Redis, RabbitMQ, and Mailpit run in containers. The APIs serve HTTPS with a dev cert mounted from `HOST_CERT_PATH`, and gRPC calls use `Grpc__AllowUntrustedServerCertificate=true`. The Functions project isn't part of compose.

**Azure Container Apps.** The APIs, the worker, and Redis run as container apps, and `Appointments.Functions` runs in a Function App. The infrastructure was created by hand, with no IaC in the repo, so new settings must also be added in Azure. The SPA isn't hosted; it runs locally on port 5500 against either backend.

## CI/CD

GitHub Actions has path-filtered CI per service (`*-ci.yml`: restore, Release build, test, SonarCloud) plus a frontend CI. The Appointments CI builds only (no tests). On `main`, the `*-cd.yml` workflows trigger via `workflow_run` after their CI succeeds. Each one builds the Docker image (context `./Backend`, NuGet credentials passed as build secrets), pushes it to GHCR tagged with the commit SHA, and runs `az containerapp update --image` after an OIDC Azure login. Appointments CD also publishes and deploys the Functions project. CD only swaps the image; env vars and secrets are managed in Azure directly. The frontend has no CD.

## Auth0

Tenant config isn't in the repo. The APIs validate the user audience `https://innocliniic/profiles` (the typo is intentional and can't be changed) and the internal gRPC audience `https://innoclinic/internal` (scopes `internal:profiles`, `internal:appointments`). Permission strings in `ClinicPermissions` must match the Auth0 API's permissions exactly. A post-login Action emits the `https://innoclinic/profile_id` and `https://innoclinic/role` claims (`ClinicClaims`). It denies login to users without `app_metadata.profile_id`/`role` and a matching RBAC role, so users must be provisioned by Profiles before they can sign in.

## Conventions

- Backend `.editorconfig` enforces LF line endings and 4-space indentation. The frontend uses Prettier with LF. `.gitattributes` (`eol=lf`) makes new checkouts LF, but files checked out earlier on Windows can still be CRLF on disk. Git normalizes them on commit, so that's harmless.
- Make hand edits to repo files with the Edit/Write tools, not shell commands (`sed`, heredocs, `Set-Content`). The hooks below only see Edit/Write, so a shell edit skips formatting and the migration guard. Files written by tools (`dotnet ef`, `npm install`, `git restore`, deleting files) are fine.
- Hooks in `.claude/settings.json` record each file Claude edits. At the end of the turn they run `dotnet format` on the edited `.cs` files (once per solution), plus `prettier --write` and an ESLint check (no `--fix`) on the edited frontend files. ESLint errors are sent back to Claude to fix. Don't format edited files by hand. Another hook blocks edits to committed EF migrations (add a new migration instead), and editing `Backend/Shared/` triggers a reminder about package versions.
- The codebase has almost no XML doc comments. Don't add `/// <summary>`. Use only short `//` notes where the reason isn't obvious.
