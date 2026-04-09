using Mapster;
using Profiles.API.Constants;
using Profiles.API.DTOs.Patient;
using Profiles.API.Filters;
using Profiles.BLL.Interfaces;
using Profiles.BLL.Models;
using Profiles.Domain.Common;

namespace Profiles.API.Endpoints;

public static class PatientEndpoints
{
    extension(IEndpointRouteBuilder routes)
    {
        public RouteGroupBuilder MapPatientEndpoints()
        {
            var group = routes.MapGroup(ApiRoutes.Patients)
                .WithTags("Patients")
                .AddEndpointFilter<ResultFilter>();

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

    private static async Task<Result<IReadOnlyList<PatientResponseDto>>> GetAllPatientsAsync(
        IPatientService patientService,
        CancellationToken ct = default)
    {
        var result = await patientService.GetAllAsync(ct);

        return result.Map(list => list.Adapt<IReadOnlyList<PatientResponseDto>>());
    }

    private static async Task<Result<PatientResponseDto>> GetPatientByIdAsync(
        Guid id,
        IPatientService patientService,
        CancellationToken ct = default)
    {
        var result = await patientService.GetByIdAsync(id, ct);

        return result.Map(p => p.Adapt<PatientResponseDto>());
    }

    private static async Task<Result<PatientResponseDto>> UpdatePatientAsync(
        Guid id,
        UpdatePatientDto dto,
        IPatientService patientService,
        CancellationToken ct = default)
    {
        var model = dto.Adapt<PatientModel>();
        var result = await patientService.UpdateAsync(id, model, ct);

        return result.Map(p => p.Adapt<PatientResponseDto>());
    }
}
