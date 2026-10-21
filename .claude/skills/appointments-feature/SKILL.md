---
name: appointments-feature
description: Add a new use case (command or query endpoint) to the Appointments service as a vertical slice — Command/Query, MediatR handler, FluentValidation validator, IEndpoint, errors, and access checks — matching the existing Features/ folders. Use when adding or changing an Appointments API endpoint.
---

# Add an Appointments feature slice

Work in `Backend/Appointments/Appointments.API/Features/<UseCaseName>/`. Before writing, read one existing slice of the same kind as a template:

- Write (EF Core): `Features/CancelAppointment/`, `Features/BookAppointment/` (validator, gRPC call, notification)
- Read (Dapper): `Features/GetPatientAppointments/`

## Files

1. **`<Name>Command.cs` / `<Name>Query.cs`**: a `record` implementing `IRequest<Result>` or `IRequest<Result<T>>` (`InnoClinic.Core.Common`). Response records live in the same file as the query.
2. **`<Name>Handler.cs`**: a primary-constructor class implementing `IRequestHandler<TRequest, Result<...>>`.
   - Writes: inject `AppointmentsDbContext`, mutate the entity, `SaveChangesAsync`.
   - Reads: inject `ISqlConnectionFactory`, use Dapper with `CommandDefinition(sql, params, cancellationToken: ...)`. Table and column names are quoted PascalCase (`""Appointments""`, `""PatientId""`).
   - Return `Error`s implicitly (`return AppointmentErrors.X;`) and `Result.Success(...)` on success. Don't throw for expected failures.
3. **`<Name>Validator.cs`** (only if the request has input to validate): `AbstractValidator<TRequest>`. It's picked up by `AddValidatorsFromAssembly`, and `ValidationBehavior` runs it before the handler. Inject `TimeProvider` for time comparisons, not `DateTime.UtcNow`.
4. **`<Name>Endpoint.cs`**: `public class <Name>Endpoint : IEndpoint`, discovered automatically by `AddEndpoints`, so don't register it in `Program.cs`. Follow this shape exactly:
   ```csharp
   app.MapPatch($"{ApiRoutes.Appointments}/{{id:guid}}/cancel", async (Guid id, ISender sender, CancellationToken ct = default) =>
       {
           var result = await sender.Send(new CancelAppointmentCommand(id), ct);
           return result;
       })
       .WithTags("Appointments")
       .RequirePermission(ClinicPermissions.Appointments.Write)
       .AddEndpointFilter<ResultFilter>();
   ```
   `ResultFilter` maps `Result`/`Error` to HTTP responses; the endpoint returns the `Result` as is.

## Errors

Add new errors to `Appointments.Domain/Common/AppointmentErrors.cs` as `Error(code, message, ErrorType)`, with codes `Appointment.<Name>`. Use a static method when the message needs an id, otherwise a `static readonly` field.

## Authorization (two layers, both required)

- **Permission** on the endpoint: `.RequirePermission(ClinicPermissions.Appointments.Read|Write|Confirm)` or `ClinicPermissions.Results.*`. `ClinicPermissions` lives in the shared `InnoClinic.AspNetCore` package. A *new* permission means a shared-package change (see the `shared-package-local` skill) **and** a matching permission on the Auth0 API plus role assignments. Ask the user before planning that.
- **Ownership/row-level** in the handler, via `IAccessResolver.Check(resource, operation).IsAllowed`:
  - Entity loaded by id → if denied, return `AppointmentErrors.NotFound(id)` (don't reveal that it exists).
  - `PatientRef` / `StaffRef` checks → if denied, return `AppointmentErrors.Forbidden`.
  - New operations go in `Authorization/AppointmentOperations.cs`; the rules go in `Authorization/AppointmentAccessPolicy.cs`. A new resource type also needs an `IAccessPolicy<T>` registration in `Program.cs`.

## Cross-service calls and notifications

- Profiles data comes from gRPC clients (`PatientService.PatientServiceClient`, `StaffScheduleSyncService.StaffScheduleSyncServiceClient`), injected into the handler.
- Emails go through `INotificationProducer.Enqueue(new <Contract>(...))` **before** `SaveChangesAsync`, so the outbox row commits in the same transaction. Contracts live in the shared `InnoClinic.Messaging` package. A new contract needs a package bump plus a consumer and template in Notifications.Worker.

## Verify

```bash
cd Backend && dotnet build InnoClinic.Appointments.slnx --no-restore
```

Appointments has no test projects. Mention to the user what you couldn't verify (e.g. the endpoint at runtime). No XML doc comments.
