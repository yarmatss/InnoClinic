using Mapster;
using Profiles.API.Constants;
using Profiles.API.DTOs.MedicalStaff;
using Profiles.API.Filters;
using Profiles.BLL.Interfaces;
using Profiles.BLL.Models;
using Profiles.Domain.Common;

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

            group.MapGet("/{id:guid}", GetStaffByIdAsync).WithName("GetStaffById");
            group.MapGet("/active", GetAllActiveStaffAsync);

            group.MapPost("/", CreateStaffAsync);

            group.MapPut("/{id:guid}", UpdateStaffAsync);
            group.MapPut("/{id:guid}/specializations", AssignSpecializationsAsync);

            group.MapDelete("/{id:guid}", DeactivateStaffAsync);
            
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

    private static async Task<Result<MedicalStaffResponseDto>> GetStaffByIdAsync(
        Guid id,
        IMedicalStaffService staffService,
        CancellationToken ct = default)
    {
        var result = await staffService.GetByIdAsync(id, ct);

        return result.Map(s => s.Adapt<MedicalStaffResponseDto>());
    }

    private static async Task<Result<IReadOnlyList<MedicalStaffResponseDto>>> GetAllActiveStaffAsync(
        IMedicalStaffService staffService,
        CancellationToken ct = default)
    {
        var result = await staffService.GetAllActiveAsync(ct);

        return result.Map(list => list.Adapt<IReadOnlyList<MedicalStaffResponseDto>>());
    }

    private static async Task<Result<MedicalStaffResponseDto>> UpdateStaffAsync(
        Guid id,
        UpdateMedicalStaffDto dto,
        IMedicalStaffService staffService,
        CancellationToken ct = default)
    {
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
        IReadOnlyList<StaffSpecializationDto> dtos,
        IMedicalStaffService staffService,
        CancellationToken ct = default)
    {
        var assignments = dtos.Adapt<List<StaffSpecializationModel>>();
        assignments.ForEach(a => a.StaffId = id);

        var result = await staffService.AssignSpecializationsAsync(id, assignments, ct);

        return result;
    }
}
