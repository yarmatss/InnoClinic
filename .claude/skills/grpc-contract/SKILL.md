---
name: grpc-contract
description: Change or add an internal gRPC contract between Profiles, Appointments, and Appointments.Functions — edit the .proto in the shared InnoClinic.Contracts.Grpc package, implement the server handler in the right service, register clients with internal M2M auth, and roll out in a compatible order. Use when a service needs new data or a new call from another service over gRPC.
---

# Change an internal gRPC contract

Contracts live in `Backend/Shared/InnoClinic.Contracts.Grpc/Protos/*.proto`, compiled with `GrpcServices="Both"` into the shared `InnoClinic.Contracts.Grpc` package (namespace `InnoClinic.Contracts.Grpc`). Who implements what:

| Service (proto) | RPC | Server | Clients |
|---|---|---|---|
| `PatientService` (`patient_service.proto`) | `GetPatient` | Profiles (`GrpcHandlers/PatientQueryHandler`) | Appointments API, Appointments.Functions |
| `StaffScheduleSyncService` (`staff_profile_sync.proto`) | `GetStaffProfile` | Profiles (`GrpcHandlers/StaffMemberQueryHandler`) | Appointments API, Appointments.Functions |
| `StaffScheduleSyncService` | `SyncStaffProfile` | Appointments (`GrpcHandlers/StaffScheduleSyncHandler`) | Profiles (`OutboxProcessorJob`) |

One proto service is implemented partly on each host. A client must point at the host that implements the RPC it calls; the other host returns `Unimplemented`.

## 1. Edit the proto (shared package)

- Keep proto3 conventions: snake_case fields, and IDs as `string` (parsed with `Guid.TryParse` on the server).
- **Compatibility:** services deploy independently, so old and new versions talk to each other during rollout.
  - You may add new fields with new numbers, and add new RPCs.
  - Never renumber or reuse a field number, and never change a field's type. To remove a field, delete it and add `reserved <n>;`.
  - Remember that an unset field arrives as its default value (`""`, `0`, `false`). Use an explicit flag like `exists` rather than relying on empty strings.
- Bump `<Version>` in `InnoClinic.Contracts.Grpc.csproj` and build the affected services with the `shared-package-local` skill. There are usually two or three: server, API client, Functions client.

## 2. Server side

- Override the generated method in the handler class (`<Service>.<Service>Base`), using a primary constructor that injects repositories.
- Bad input → `throw new RpcException(new Status(StatusCode.InvalidArgument, "..."))`. A missing entity is usually a normal response with `Exists = false`, not an error, so callers can map it to their own `NotFound`.
- A new service class is mapped in `Program.cs`, and **must** require the internal scope of the host it runs on:
  ```csharp
  app.MapGrpcService<XHandler>().RequireInternalScope(InternalScopes.Profiles);   // or InternalScopes.Appointments
  ```

## 3. Client side

- Register clients with `services.AddInternalGrpcClient<XService.XServiceClient>(address, configuration)` (from `InnoClinic.Contracts.Grpc/Auth`). This attaches an M2M token from the `Auth0:Services` client and honors `Grpc:AllowUntrustedServerCertificate`. Existing registrations:
  - `Appointments.Infrastructure/DependencyInjection.cs`
  - `Appointments.Functions/DependencyInjection.cs`
  - `Profiles.DAL/DependencyInjection.cs`
- Addresses come from config: `ProfilesApiUrl` and `AppointmentsApiUrl`. A **new** address or setting has to be added to docker-compose **and** by hand in Azure (Container Apps env and Function App settings). Tell the user; there's no IaC.
- Handle `RpcException` at the call site where a failure must not break the caller (see `OutboxProcessorJob`).

## 4. Rollout order (tell the user)

1. Publish the new `InnoClinic.Contracts.Grpc` and update `Directory.Packages.props`.
2. Deploy the **server** first.
3. Then deploy the clients. Appointments CD deploys both the API and the Functions app.

Calling a new RPC or relying on a new field before the server has it gives `Unimplemented` or default values.

## Verify

```bash
cd Backend
dotnet build InnoClinic.Profiles.slnx --no-restore
dotnet build InnoClinic.Appointments.slnx --no-restore   # includes Appointments.Functions
dotnet test --project Profiles/Profiles.UnitTests --no-build
```

There are no gRPC integration tests. An end-to-end check needs docker compose with both APIs running. Say what wasn't run.
