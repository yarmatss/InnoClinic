using FluentValidation;
using Mapster;
using InnoClinic.AspNetCore.Authorization;
using Profiles.API.Constants;
using Profiles.API.DTOs.Patient;
using InnoClinic.AspNetCore.Filters;
using Profiles.BLL.Interfaces;
using Profiles.BLL.Models;
using InnoClinic.Core.Common;
using Profiles.Domain.Models;

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

            group.MapGet("/me", GetCurrentPatientAsync)
                .RequireAuthorization();

            group.MapGet("/", GetAllPatientsAsync)
                .RequirePermission(ClinicPermissions.Patients.Read);
            group.MapGet("/{id:guid}", GetPatientByIdAsync)
                .RequirePermission(ClinicPermissions.Patients.Read);

            group.MapPost("/", CreatePatientAsync)
                .RequirePermission(ClinicPermissions.Patients.Write);
            
            group.MapPut("/{id:guid}", UpdatePatientAsync)
                .RequirePermission(ClinicPermissions.Patients.Write);

            return group;
        }
    }

    private static async Task<Result<PatientResponseDto>> CreatePatientAsync(
        CreatePatientDto dto,
        IValidator<CreatePatientDto> validator,
        IPatientService patientService,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return new ValidationError(validationResult.ToDictionary());
        }

        var model = dto.Adapt<PatientModel>();
        var result = await patientService.CreateAsync(model, ct);

        return result.Map(p => 
        {
            var responseDto = p.Adapt<PatientResponseDto>();
            return Result.Created(responseDto, $"{ApiRoutes.Patients}/{responseDto.Id}");
        });
    }

    private static async Task<Result<PagedResponse<PatientResponseDto>>> GetAllPatientsAsync(
        [AsParameters] PatientQueryParameters query,
        IValidator<PatientQueryParameters> validator,
        IPatientService patientService,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return new ValidationError(validationResult.ToDictionary());
        }

        var result = await patientService.GetAllAsync(query, ct);

        return result.Map(pagedModel => new PagedResponse<PatientResponseDto>
        {
            Items = pagedModel.Items.Adapt<IReadOnlyList<PatientResponseDto>>(),
            TotalCount = pagedModel.TotalCount,
            PageNumber = pagedModel.PageNumber,
            PageSize = pagedModel.PageSize
        });
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
        IValidator<UpdatePatientDto> validator,
        IPatientService patientService,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return new ValidationError(validationResult.ToDictionary());
        }

        var model = dto.Adapt<PatientModel>();
        var result = await patientService.UpdateAsync(id, model, ct);

        return result.Map(p => p.Adapt<PatientResponseDto>());
    }

    private static async Task<Result<PatientResponseDto>> GetCurrentPatientAsync(
        IPatientService patientService,
        CancellationToken ct = default)
    {
        var result = await patientService.GetCurrentAsync(ct);

        return result.Map(p => p.Adapt<PatientResponseDto>());
    }
}
