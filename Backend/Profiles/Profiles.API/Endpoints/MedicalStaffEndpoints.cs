using FluentValidation;
using InnoClinic.AspNetCore.Authorization;
using InnoClinic.AspNetCore.Filters;
using InnoClinic.Core.Common;
using Mapster;
using Profiles.API.Constants;
using Profiles.API.DTOs.MedicalStaff;
using Profiles.BLL.Interfaces;
using Profiles.BLL.Models;
using Profiles.Domain.Models;

namespace Profiles.API.Endpoints;

public static class MedicalStaffEndpoints
{
    extension(IEndpointRouteBuilder routes)
    {
        public RouteGroupBuilder MapMedicalStaffEndpoints()
        {
            var group = routes.MapGroup(ApiRoutes.MedicalStaff)
                .WithTags("Medical Staff")
                .AddEndpointFilter<ResultFilter>();

            group.MapGet("/me", GetCurrentStaffAsync)
                .RequireAuthorization();

            group.MapGet("/{id:guid}", GetStaffByIdAsync)
                .WithName("GetStaffById")
                .RequirePermission(ClinicPermissions.Staff.Read);
            group.MapGet("/active", GetAllActiveStaffAsync)
                .RequirePermission(ClinicPermissions.Staff.Read);

            group.MapPost("/", CreateStaffAsync)
                .RequirePermission(ClinicPermissions.Staff.Write);

            group.MapPut("/{id:guid}", UpdateStaffAsync)
                .RequirePermission(ClinicPermissions.Staff.Write);
            group.MapPut("/{id:guid}/specializations", AssignSpecializationsAsync)
                .RequirePermission(ClinicPermissions.Staff.Write);
            group.MapPut("/{id:guid}/working-hours", SetWorkingHoursAsync)
                .RequirePermission(ClinicPermissions.Staff.Write);
            group.MapPut("/{id:guid}/schedule-overrides", SetScheduleOverridesAsync)
                .RequirePermission(ClinicPermissions.Staff.Write);

            group.MapDelete("/{id:guid}/schedule-overrides/{date}", DeleteScheduleOverrideAsync)
                .RequirePermission(ClinicPermissions.Staff.Write);
            group.MapDelete("/{id:guid}", DeactivateStaffAsync)
                .RequirePermission(ClinicPermissions.Staff.Write);

            return group;
        }
    }

    private static async Task<Result<MedicalStaffResponseDto>> CreateStaffAsync(
        CreateMedicalStaffDto dto,
        IValidator<CreateMedicalStaffDto> validator,
        IMedicalStaffService staffService,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return new ValidationError(validationResult.ToDictionary());
        }

        var model = dto.Adapt<MedicalStaffModel>();
        var result = await staffService.CreateAsync(model, ct);

        return result.Map(s =>
        {
            var responseDto = s.Adapt<MedicalStaffResponseDto>();
            return Result.Created(responseDto, $"{ApiRoutes.MedicalStaff}/{responseDto.Id}");
        });
    }

    private static async Task<Result<MedicalStaffResponseDto>> GetStaffByIdAsync(
        Guid id,
        IMedicalStaffService staffService,
        CancellationToken ct = default)
    {
        var result = await staffService.GetByIdAsync(id, ct);

        return result.Map(s => s.Adapt<MedicalStaffResponseDto>());
    }

    private static async Task<Result<PagedResponse<MedicalStaffResponseDto>>> GetAllActiveStaffAsync(
        [AsParameters] MedicalStaffQueryParameters query,
        IValidator<MedicalStaffQueryParameters> validator,
        IMedicalStaffService medicalStaffService,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return new ValidationError(validationResult.ToDictionary());
        }

        var result = await medicalStaffService.GetPagedAsync(query, ct);

        return result.Map(pagedModel => new PagedResponse<MedicalStaffResponseDto>
        {
            Items = pagedModel.Items.Adapt<IReadOnlyList<MedicalStaffResponseDto>>(),
            TotalCount = pagedModel.TotalCount,
            PageNumber = pagedModel.PageNumber,
            PageSize = pagedModel.PageSize
        });
    }

    private static async Task<Result<MedicalStaffResponseDto>> UpdateStaffAsync(
        Guid id,
        UpdateMedicalStaffDto dto,
        IValidator<UpdateMedicalStaffDto> validator,
        IMedicalStaffService staffService,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return new ValidationError(validationResult.ToDictionary());
        }

        var model = dto.Adapt<MedicalStaffModel>();
        var result = await staffService.UpdateAsync(id, model, ct);

        return result.Map(s => s.Adapt<MedicalStaffResponseDto>());
    }

    private static async Task<Result> DeactivateStaffAsync(
        Guid id,
        IMedicalStaffService staffService,
        CancellationToken ct = default)
    {
        var result = await staffService.DeactivateAsync(id, ct);

        return result;
    }

    private static async Task<Result> AssignSpecializationsAsync(
        Guid id,
        AssignSpecializationsDto request,
        IValidator<AssignSpecializationsDto> validator,
        IMedicalStaffService staffService,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return new ValidationError(validationResult.ToDictionary());

        var assignments = request.Specializations.Adapt<List<StaffSpecializationModel>>();
        assignments.ForEach(a => a.StaffId = id);

        return await staffService.AssignSpecializationsAsync(id, assignments, ct);
    }

    private static async Task<Result> SetWorkingHoursAsync(
        Guid id,
        SetWorkingHoursDto request,
        IValidator<SetWorkingHoursDto> validator,
        IMedicalStaffService staffService,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return new ValidationError(validationResult.ToDictionary());

        var workingHours = request.WorkingHours.Adapt<List<WorkingHoursModel>>();

        return await staffService.SetWorkingHoursAsync(id, workingHours, ct);
    }

    private static async Task<Result> SetScheduleOverridesAsync(
        Guid id,
        SetScheduleOverridesDto request,
        IValidator<SetScheduleOverridesDto> validator,
        IMedicalStaffService staffService,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return new ValidationError(validationResult.ToDictionary());

        var overrides = request.Overrides.Adapt<List<ScheduleOverrideModel>>();

        return await staffService.SetScheduleOverridesAsync(id, overrides, ct);
    }

    private static async Task<Result> DeleteScheduleOverrideAsync(
        Guid id,
        DateOnly date,
        IMedicalStaffService staffService,
        CancellationToken ct = default)
    {
        return await staffService.DeleteScheduleOverrideAsync(id, date, ct);
    }

    private static async Task<Result<MedicalStaffResponseDto>> GetCurrentStaffAsync(
        IMedicalStaffService staffService,
        CancellationToken ct = default)
    {
        var result = await staffService.GetCurrentAsync(ct);

        return result.Map(s => s.Adapt<MedicalStaffResponseDto>());
    }
}
