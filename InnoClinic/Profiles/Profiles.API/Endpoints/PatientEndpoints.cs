using Mapster;
using Profiles.API.Constants;
using Profiles.API.DTOs.Patient;
using Profiles.BLL.Interfaces;
using Profiles.BLL.Models;

namespace Profiles.API.Endpoints;

public static class PatientEndpoints
{
    extension(IEndpointRouteBuilder routes)
    {
        public RouteGroupBuilder MapPatientEndpoints()
        {
            var group = routes.MapGroup(ApiRoutes.Patients).WithTags("Patients");

            group.MapGet("/", GetAllPatientsAsync);
            group.MapGet("/{id:guid}", GetPatientByIdAsync);

            group.MapPost("/", CreatePatientAsync);
            
            group.MapPut("/{id:guid}", UpdatePatientAsync);

            return group;
        }
    }

    private static async Task<IResult> CreatePatientAsync(
        CreatePatientDto dto,
        IPatientService patientService,
        CancellationToken ct = default)
    {
        var model = dto.Adapt<PatientModel>();
        var result = await patientService.CreateAsync(model, ct);

        if (result.IsFailure)
            return TypedResults.BadRequest(result.Error);

        var responseDto = result.Value.Adapt<PatientResponseDto>();
        return TypedResults.Created($"{ApiRoutes.Patients}/{responseDto.Id}", responseDto);
    }

    private static async Task<IResult> GetAllPatientsAsync(
        IPatientService patientService,
        CancellationToken ct = default)
    {
        var result = await patientService.GetAllAsync(ct);

        if (result.IsFailure)
            return TypedResults.BadRequest(result.Error);

        var responseDtos = result.Value.Adapt<IReadOnlyList<PatientResponseDto>>();
        return TypedResults.Ok(responseDtos);
    }

    private static async Task<IResult> GetPatientByIdAsync(
        Guid id,
        IPatientService patientService,
        CancellationToken ct = default)
    {
        var result = await patientService.GetByIdAsync(id, ct);

        if (result.IsFailure)
            return TypedResults.BadRequest(result.Error);

        return TypedResults.Ok(result.Value.Adapt<PatientResponseDto>());
    }

    private static async Task<IResult> UpdatePatientAsync(
        Guid id,
        UpdatePatientDto dto,
        IPatientService patientService,
        CancellationToken ct = default)
    {
        var model = dto.Adapt<PatientModel>();
        var result = await patientService.UpdateAsync(id, model, ct);

        if (result.IsFailure)
            return TypedResults.BadRequest(result.Error);

        return TypedResults.Ok(result.Value.Adapt<PatientResponseDto>());
    }
}
