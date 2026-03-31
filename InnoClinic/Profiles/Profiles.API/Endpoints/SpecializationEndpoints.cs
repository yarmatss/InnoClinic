using Mapster;
using Profiles.API.Constants;
using Profiles.API.DTOs.Specialization;
using Profiles.BLL.Interfaces;
using Profiles.BLL.Models;

namespace Profiles.API.Endpoints;

public static class SpecializationEndpoints
{
    extension(IEndpointRouteBuilder routes)
    {
        public RouteGroupBuilder MapSpecializationEndpoints()
        {
            var group = routes.MapGroup(ApiRoutes.Specializations).WithTags("Specializations");

            group.MapGet("/", GetAllSpecializationsAsync);

            group.MapPost("/", CreateSpecializationAsync);
            
            group.MapPut("/{id:guid}", UpdateSpecializationAsync);

            return group;
        }
    }

    private static async Task<IResult> CreateSpecializationAsync(
        CreateSpecializationDto dto,
        ISpecializationService specializationService,
        CancellationToken ct = default)
    {
        var model = dto.Adapt<SpecializationModel>();
        var result = await specializationService.CreateAsync(model, ct);

        if (result.IsFailure)
            return TypedResults.BadRequest(result.Error);

        var responseDto = result.Value.Adapt<SpecializationResponseDto>();
        return TypedResults.Created($"{ApiRoutes.Specializations}/{responseDto.Id}", responseDto);
    }

    private static async Task<IResult> GetAllSpecializationsAsync(
        ISpecializationService specializationService,
        CancellationToken ct = default)
    {
        var result = await specializationService.GetAllAsync(ct);

        if (result.IsFailure)
            return TypedResults.BadRequest(result.Error);

        var responseDtos = result.Value.Adapt<IReadOnlyList<SpecializationResponseDto>>();
        return TypedResults.Ok(responseDtos);
    }

    private static async Task<IResult> UpdateSpecializationAsync(
        Guid id,
        UpdateSpecializationDto dto,
        ISpecializationService specializationService,
        CancellationToken ct = default)
    {
        var model = dto.Adapt<SpecializationModel>();
        var result = await specializationService.UpdateAsync(id, model, ct);

        if (result.IsFailure)
            return TypedResults.BadRequest(result.Error);

        return TypedResults.Ok(result.Value.Adapt<SpecializationResponseDto>());
    }
}
