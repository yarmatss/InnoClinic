# InnoClinic

Clinic management system: .NET 10 microservices backend + React SPA frontend. Auth via Auth0.

## Layout

- `Backend/` — one solution per service, run commands from here.
  - `Profiles/` (`InnoClinic.Profiles.slnx`) — patients, medical staff, specializations. Layered: API → BLL → DAL → Domain. Unit and integration tests live here.
  - `Appointments/` (`InnoClinic.Appointments.slnx`) — vertical slices under `Appointments.API/Features/`, plus `Appointments.Functions` (Azure Functions reminder cron).
  - `Notifications/` (`InnoClinic.Notifications.slnx`) — `Notifications.Worker` consuming RabbitMQ messages and sending email.
  - `Shared/` — `InnoClinic.Core`, `InnoClinic.AspNetCore`, `InnoClinic.Contracts.Grpc`, `InnoClinic.Messaging`.
- `Frontend/` — React + Vite + TypeScript + MUI, structured with Feature-Sliced Design (`app`, `pages`, `widgets`, `features`, `entities`, `shared`).

## Commands

Backend (from `Backend/`):
- Build: `dotnet build InnoClinic.Profiles.slnx` (same for Appointments, Notifications)
- Test: `dotnet test --solution InnoClinic.Profiles.slnx` — integration tests use Testcontainers and need Docker running.
- Format: `dotnet format InnoClinic.Profiles.slnx`
- Local stack: `docker compose up` (Postgres, Redis, RabbitMQ, MailHog, services)

Frontend (from `Frontend/`):
- `npm run dev`, `npm run build`
- `npm run lint`, `npm run lint:fsd` (steiger FSD rules), `npm run format:check`

A husky pre-commit hook runs lint-staged and `lint:fsd`. A Claude Code hook formats each file after it is edited.

## Things that aren't obvious

- Shared libraries are consumed as **NuGet packages** from the GitHub feed (`Backend/nuget.config`, `NUGET_USER`/`NUGET_TOKEN`), not project references. Versions are in `Backend/Directory.Packages.props` (central package management). A change in `Shared/` needs a version bump and publish before services pick it up. Never publish to the feed without asking.
- Authorization: permissions are checked at endpoints; ownership/resource access goes through `IAccessResolver` and per-resource access policies. Service-to-service gRPC uses Auth0 M2M tokens with `internal:*` scopes on a separate Internal auth scheme.
- Auth0 role and `profile_id` live in `app_metadata` and must stay in sync with the Auth0 RBAC role during provisioning.
- Backend files use LF line endings (`Backend/.editorconfig`); usings are sorted without putting `System` first.

## Conventions

- No `///` XML doc comments. Keep comments to short `//` notes where the reason isn't obvious.
- Match the style of surrounding code; keep changes minimal and explain any change to auth or Auth0 configuration.
