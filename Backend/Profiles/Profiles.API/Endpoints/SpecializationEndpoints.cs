using FluentValidation;
using Mapster;
using Profiles.API.Authorization;
using Profiles.API.Constants;
using Profiles.API.DTOs.Specialization;
using Profiles.API.Filters;
using Profiles.BLL.Interfaces;
using Profiles.BLL.Models;
using Profiles.Domain.Common;
using Profiles.Domain.Models;

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

            group.MapGet("/", GetAllSpecializationsAsync)
                .AllowAnonymous();

            group.MapPost("/", CreateSpecializationAsync)
                .RequireAuthorization(Policies.WriteSpecializations);
            
            group.MapPut("/{id:guid}", UpdateSpecializationAsync)
                .RequireAuthorization(Policies.WriteSpecializations);

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

    private static async Task<Result<PagedResponse<SpecializationResponseDto>>> GetAllSpecializationsAsync(
        [AsParameters] SpecializationQueryParameters query,
        IValidator<SpecializationQueryParameters> validator,
        ISpecializationService specializationService,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return new ValidationError(validationResult.ToDictionary());
        }

        var result = await specializationService.GetPagedAsync(query, ct);

        return result.Map(pagedModel => new PagedResponse<SpecializationResponseDto>
        {
            Items = pagedModel.Items.Adapt<IReadOnlyList<SpecializationResponseDto>>(),
            TotalCount = pagedModel.TotalCount,
            PageNumber = pagedModel.PageNumber,
            PageSize = pagedModel.PageSize
        });
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
