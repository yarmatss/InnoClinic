using Mapster;
using Profiles.API.Constants;
using Profiles.API.DTOs.Specialization;
using Profiles.API.Filters;
using Profiles.BLL.Interfaces;
using Profiles.BLL.Models;
using Profiles.Domain.Common;

namespace Profiles.API.Endpoints;

public static class SpecializationEndpoints
{
    extension(IEndpointRouteBuilder routes)
    {
        public RouteGroupBuilder MapSpecializationEndpoints()
        {
            var group = routes.MapGroup(ApiRoutes.Specializations)
                .WithTags("Specializations")
                .AddEndpointFilter<ResultFilter>();

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

    private static async Task<Result<IReadOnlyList<SpecializationResponseDto>>> GetAllSpecializationsAsync(
        ISpecializationService specializationService,
        CancellationToken ct = default)
    {
        var result = await specializationService.GetAllAsync(ct);

        return result.Map(list => list.Adapt<IReadOnlyList<SpecializationResponseDto>>());
    }

    private static async Task<Result<SpecializationResponseDto>> UpdateSpecializationAsync(
        Guid id,
        UpdateSpecializationDto dto,
        ISpecializationService specializationService,
        CancellationToken ct = default)
    {
        var model = dto.Adapt<SpecializationModel>();
        var result = await specializationService.UpdateAsync(id, model, ct);

        return result.Map(m => m.Adapt<SpecializationResponseDto>());
    }
}
