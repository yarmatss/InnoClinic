using FluentValidation;
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

    private static async Task<Result<SpecializationResponseDto>> CreateSpecializationAsync(
        CreateSpecializationDto dto,
        IValidator<CreateSpecializationDto> validator,
        ISpecializationService specializationService,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return new ValidationError(validationResult.ToDictionary());
        }

        var model = dto.Adapt<SpecializationModel>();
        var result = await specializationService.CreateAsync(model, ct);

        return result.Map(m => 
        {
            var responseDto = m.Adapt<SpecializationResponseDto>();
            return Result.Created(responseDto, $"{ApiRoutes.Specializations}/{responseDto.Id}");
        });
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
        IValidator<UpdateSpecializationDto> validator,
        ISpecializationService specializationService,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return new ValidationError(validationResult.ToDictionary());
        }

        var model = dto.Adapt<SpecializationModel>();
        var result = await specializationService.UpdateAsync(id, model, ct);

        return result.Map(m => m.Adapt<SpecializationResponseDto>());
    }
}
