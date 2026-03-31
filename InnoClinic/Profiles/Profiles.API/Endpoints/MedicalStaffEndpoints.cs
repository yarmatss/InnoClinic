using Mapster;
using Profiles.API.Constants;
using Profiles.API.DTOs.MedicalStaff;
using Profiles.BLL.Interfaces;
using Profiles.BLL.Models;

namespace Profiles.API.Endpoints;

public static class MedicalStaffEndpoints
{
    extension(IEndpointRouteBuilder routes)
    {
        public RouteGroupBuilder MapMedicalStaffEndpoints()
        {
            var group = routes.MapGroup(ApiRoutes.MedicalStaff).WithTags("Medical Staff");

            group.MapPost("/", CreateStaffAsync);
            group.MapGet("/{id:guid}", GetStaffByIdAsync).WithName("GetStaffById");
            group.MapGet("/active", GetAllActiveStaffAsync);
            group.MapPut("/{id:guid}", UpdateStaffAsync);
            group.MapDelete("/{id:guid}", DeactivateStaffAsync);
            group.MapPut("/{id:guid}/specializations", AssignSpecializationsAsync);

            return group;
        }
    }

    private static async Task<IResult> CreateStaffAsync(
        CreateMedicalStaffDto dto,
        IMedicalStaffService staffService,
        CancellationToken ct = default)
    {
        var model = dto.Adapt<MedicalStaffModel>();
        var result = await staffService.CreateAsync(model, ct);

        if (result.IsFailure)
            return TypedResults.BadRequest(result.Error);

        var responseDto = result.Value.Adapt<MedicalStaffResponseDto>();
        return TypedResults.CreatedAtRoute(responseDto, "GetStaffById", new { id = responseDto.Id });
    }

    private static async Task<IResult> GetStaffByIdAsync(
        Guid id,
        IMedicalStaffService staffService,
        CancellationToken ct = default)
    {
        var result = await staffService.GetByIdAsync(id, ct);

        if (result.IsFailure)
            return TypedResults.BadRequest(result.Error);

        return TypedResults.Ok(result.Value.Adapt<MedicalStaffResponseDto>());
    }

    private static async Task<IResult> GetAllActiveStaffAsync(
        IMedicalStaffService staffService,
        CancellationToken ct = default)
    {
        var result = await staffService.GetAllActiveAsync(ct);

        if (result.IsFailure)
            return TypedResults.BadRequest(result.Error);

        var responseDtos = result.Value.Adapt<IReadOnlyList<MedicalStaffResponseDto>>();
        return TypedResults.Ok(responseDtos);
    }

    private static async Task<IResult> UpdateStaffAsync(
        Guid id,
        UpdateMedicalStaffDto dto,
        IMedicalStaffService staffService,
        CancellationToken ct = default)
    {
        var model = dto.Adapt<MedicalStaffModel>();
        var result = await staffService.UpdateAsync(id, model, ct);

        if (result.IsFailure)
            return TypedResults.BadRequest(result.Error);

        return TypedResults.Ok(result.Value.Adapt<MedicalStaffResponseDto>());
    }

    private static async Task<IResult> DeactivateStaffAsync(
        Guid id,
        IMedicalStaffService staffService,
        CancellationToken ct = default)
    {
        var result = await staffService.DeactivateAsync(id, ct);

        if (result.IsFailure)
            return TypedResults.BadRequest(result.Error);

        return TypedResults.NoContent();
    }

    private static async Task<IResult> AssignSpecializationsAsync(
        Guid id,
        IReadOnlyList<StaffSpecializationDto> dtos,
        IMedicalStaffService staffService,
        CancellationToken ct = default)
    {
        var assignments = dtos.Adapt<List<StaffSpecializationModel>>();
        assignments.ForEach(a => a.StaffId = id);

        var result = await staffService.AssignSpecializationsAsync(id, assignments, ct);

        if (result.IsFailure)
            return TypedResults.BadRequest(result.Error);

        return TypedResults.NoContent();
    }
}
