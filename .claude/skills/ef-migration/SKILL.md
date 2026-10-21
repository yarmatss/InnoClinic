---
name: ef-migration
description: Add, review, or undo an EF Core migration for the Profiles or Appointments service with the correct project, startup project, and output folder, then show the generated SQL. Use whenever an entity, DbContext, or EF configuration change needs a migration.
---

# EF Core migrations

Run from `Backend/`. `dotnet-ef` is a global tool. Its version warning about the runtime is harmless.

| Service | DbContext | `--project` | `--startup-project` | Extra |
|---|---|---|---|---|
| Profiles | `ProfilesDbContext` | `Profiles/Profiles.DAL` | `Profiles/Profiles.API` | — |
| Appointments | `AppointmentsDbContext` | `Appointments/Appointments.Infrastructure` | `Appointments/Appointments.API` | `--output-dir Data/Migrations` |

Appointments keeps its migrations in `Data/Migrations`; without `--output-dir`, a new migration lands in a new `Migrations/` folder.

## Add

```bash
dotnet ef migrations add <PascalCaseName> --project Profiles/Profiles.DAL --startup-project Profiles/Profiles.API
dotnet ef migrations add <PascalCaseName> --project Appointments/Appointments.Infrastructure --startup-project Appointments/Appointments.API --output-dir Data/Migrations
```

Name the migration for the change (`AddNotificationOutbox`, `MakeUserIdRequired`). Adding doesn't need a database.

## Review: always show the SQL

```bash
dotnet ef migrations script <PreviousMigration> <NewMigration> --project ... --startup-project ...
```

Don't pass `--no-build`: the new migration isn't compiled yet, so the script would fail with "migration not found". Read the generated `.cs` and SQL and point out to the user anything destructive (dropped columns or tables, type changes, non-null columns added without defaults on existing data). An empty `Up()` means the model didn't change.

## Applying

Don't run `dotnet ef database update`. Migrations are applied automatically on startup in Development (`ApplyMigrationsAsync`), locally via docker compose and in Azure. A migration therefore reaches the shared Azure database as soon as that image is deployed, so tell the user about anything destructive before they merge.

## Undo a migration that was never applied

`dotnet ef migrations remove` connects to the database to check whether the migration was applied. It fails if the database isn't running ("ConnectionString property has not been initialized" or "Failed to connect"). For a migration that was just added and never applied:

```bash
rm <MigrationsDir>/<timestamp>_<Name>.cs <MigrationsDir>/<timestamp>_<Name>.Designer.cs
git restore -- <MigrationsDir>/<Context>ModelSnapshot.cs
```

Only `git restore` the snapshot if it had no other uncommitted changes before the migration was added; otherwise use `migrations remove` with the local database running (`docker compose up -d postgres`).

## Verify

```bash
dotnet build InnoClinic.<Service>.slnx --no-restore
```

For Profiles, also run the integration tests (Docker required); they build the schema from the migrations.
