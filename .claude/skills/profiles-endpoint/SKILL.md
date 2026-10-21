---
name: profiles-endpoint
description: Add or change an endpoint in the Profiles service through all layers — DTO, FluentValidation validator, Mapster mapping, BLL model/service/errors/access check, DAL repository/query builder, minimal-API endpoint — plus unit and integration tests in the existing style. Use when adding or changing Profiles API behavior (patients, medical staff, specializations).
---

# Add a Profiles endpoint

Profiles is layered: `Profiles.API` → `Profiles.BLL` → `Profiles.DAL` → `Profiles.Domain`. Specialization is the smallest complete example; read its files in each layer before writing:
`API/Endpoints/SpecializationEndpoints.cs`, `API/DTOs/Specialization/`, `API/Validators/Specialization/`, `BLL/Services/SpecializationService.cs`, `BLL/Errors/SpecializationErrors.cs`, `DAL/Repositories/SpecializationRepository.cs`, `DAL/Builders/SpecializationQueryBuilder.cs`.

## API layer (`Profiles.API`)

- **DTOs** (`DTOs/<Area>/`): positional `record`s: `Create<X>Dto`, `Update<X>Dto`, `<X>ResponseDto`.
- **Validators** (`Validators/<Area>/`): `AbstractValidator<TDto>`, registered automatically. Reusable property rules go in `<Area>PropertyValidators.cs` as `internal` validators. Paged queries `Include(new PaginationQueryParametersValidator<T>())`.
- **Endpoint** (`Endpoints/<Area>Endpoints.cs`): add a route to the existing `Map<Area>Endpoints()` group, using a `private static async Task<Result<T>>` handler method. Every handler validates explicitly, then calls the service:
  ```csharp
  var validationResult = await validator.ValidateAsync(dto, ct);
  if (!validationResult.IsValid)
      return new ValidationError(validationResult.ToDictionary());

  var result = await service.CreateAsync(dto.Adapt<XModel>(), ct);
  return result.Map(m => m.Adapt<XResponseDto>());
  ```
  - For creates, return `Result.Created(dto, $"{ApiRoutes.X}/{dto.Id}")` inside `Map`.
  - The group already has `ResultFilter`; don't add it per route.
  - Authorization goes on the route: `.RequirePermission(ClinicPermissions.X.Y)`, or `.RequireAuthorization()` for reads open to any signed-in user.
  - A new area needs a constant in `Constants/ApiRoutes.cs` and a `Map<Area>Endpoints()` call in `Program.cs`.
- **Mapping:** Mapster maps same-named properties by convention. Only add an `IRegister` in `Mapping/Registers/` when a mapping needs configuration (ignored members, renames, conversions). Registers are scanned at startup.

## BLL layer (`Profiles.BLL`)

- **Model** (`Models/<X>Model.cs`): a class deriving from `BaseModel` (`Id`).
- **Service:** an interface in `Interfaces/`, and an `internal` implementation in `Services/` with a primary constructor. Register it in `BLL/DependencyInjection.cs` (`AddScoped`). Methods return `Result<T>`: return errors implicitly, never throw for expected failures.
- **Errors** (`Errors/<X>Errors.cs`): `class <X>Errors : DomainErrors`, using `CreateNotFound(nameof(Entity))`, `CreateConflict("X.Code", "...")`, `CreateForbidden("X.Forbidden", "...")`.
- **Access checks:** Profiles checks access **before** loading data and returns `Forbidden`. This is the opposite order from Appointments, which returns `NotFound`:
  ```csharp
  var access = accessResolver.Check(new PatientRef(id), Operation.Read);
  if (!access.IsAllowed)
      return PatientErrors.Forbidden;
  ```
  - `AccessLevel.Masked` means a doctor reading a patient: return the data with sensitive fields masked (see `PatientService.MaskSensitivePatientFields`).
  - Policies live in `BLL/Authorization/`, registered as singletons in `BLL/DependencyInjection.cs`.
- **Side effects:** account provisioning goes through `IAuthManagementService` (Auth0). Notifications go through `INotificationProducer.Enqueue(...)`, and staff changes that Appointments needs go to the gRPC sync outbox (`IOutboxRepository`). Queue them before `SaveChangesAsync` so they commit together. See `MedicalStaffService`.

## DAL layer (`Profiles.DAL`)

- **Repositories:** derive from `BaseRepository<T>`, which provides `GetByIdAsync`, `GetByConditionAsync`, `MarkAdd/MarkUpdate/MarkDelete` and `SaveChangesAsync`. Add only the specific queries the service needs. Pass `trackChanges: true` only when the entity will be updated. Register new repositories in `DAL/DependencyInjection.cs`.
- **Filtering and sorting:** go through an `internal` query builder in `Builders/` (`EF.Functions.ILike` for text filters). Paging uses `ApplyPagination`. Query parameter classes live in `Profiles.Domain/Models` and derive from `PaginationParameters`.
- **Schema changes:** entity configuration goes in `Data/Configuration/`, then add a migration with the `ef-migration` skill.

## Tests (required)

- **Unit** (`Profiles.UnitTests/Services/<X>ServiceTests.cs`):
  - Construct the service directly with `Substitute.For<IRepo>()` (NSubstitute) and assert with Shouldly.
  - Use `// Arrange / // Act / // Assert` comments and group tests with `#region <MethodName>`.
  - Name tests `Method_Condition_ExpectedResult`.
  - Fakes come from Bogus: entities in `Profiles.Tests.Common/Fakes/Entities`, models in `Profiles.UnitTests/Fakes/Models`.
  - For access checks, use `new TestAccessResolver(user, new XAccessPolicy())`. `TestCurrentUser.Administrator()` is the only factory; build other roles with `new TestCurrentUser("test-user", UserRole.Patient, profileId)`.
- **Integration** (`Profiles.IntegrationTests/Endpoints/<Area>EndpointsTests.cs`):
  - Use `[Collection("IntegrationTests")]` and derive from `IntegrationTestBase(dbFixture)`. The database is reset before each test, and an admin staff member is seeded.
  - Seed data through `DbFixture.CreateDbContext()` and call the API with `Client`, passing `TestContext.Current.CancellationToken`.
  - `TestAuthHandler` defaults to Administrator with `ClinicPermissions.All`. Override per request with `X-Test-Role`, `X-Test-Profile-Id`, `X-Test-Permission` (repeatable), or `X-Test-Anonymous`.
  - Cover the success status, a validation 400, and the 403/404 paths.
- A **new permission** means changing `ClinicPermissions` (and its `All` list) in the shared `InnoClinic.AspNetCore` package (`shared-package-local` skill), plus adding it to the Auth0 API and roles. Ask the user before planning that.

## Verify

```bash
cd Backend
dotnet build InnoClinic.Profiles.slnx --no-restore
dotnet test --project Profiles/Profiles.UnitTests --no-build
dotnet test --project Profiles/Profiles.IntegrationTests --no-build   # needs Docker Desktop running
```

If Docker isn't running, say the integration tests weren't run. Don't count them as passing. No XML doc comments.
