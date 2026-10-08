using FluentValidation;
using InnoClinic.AspNetCore.Extensions;
using InnoClinic.AspNetCore.Filters;
using InnoClinic.Core.Common;
using Mapster;
using Profiles.API.Authorization;
using Profiles.API.Constants;
using Profiles.API.DTOs.Specialization;
using Profiles.BLL.Errors;
using Profiles.BLL.Interfaces;
using Profiles.BLL.Models;
using Profiles.Domain.Models;
using System.Security.Claims;

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
                .RequireAuthorization();

            group.MapPost("/", CreateSpecializationAsync)
                .RequireAuthorization(Policies.WriteSpecializations);
            
            group.MapPut("/{id:guid}", UpdateSpecializationAsync)
                .RequireAuthorization(Policies.WriteSpecializations);

            return group;
        }
    }

    private static async Task<Result<SpecializationResponseDto>> CreateSpecializationAsync(
        CreateSpecializationDto dto,
        ClaimsPrincipal user,
        IValidator<CreateSpecializationDto> validator,
        ISpecializationService specializationService,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return new ValidationError(validationResult.ToDictionary());
        }

        var userId = user.GetUserId();
        if (string.IsNullOrWhiteSpace(userId)) 
            return SpecializationErrors.Unauthorized;

        var model = dto.Adapt<SpecializationModel>();
        var result = await specializationService.CreateAsync(model, userId, ct);

        return result.Map(m => 
        {
            var responseDto = m.Adapt<SpecializationResponseDto>();
            return Result.Created(responseDto, $"{ApiRoutes.Specializations}/{responseDto.Id}");
        });
    }

    private static async Task<Result<PagedResponse<SpecializationResponseDto>>> GetAllSpecializationsAsync(
        [AsParameters] SpecializationQueryParameters query,
        ClaimsPrincipal user,
        IValidator<SpecializationQueryParameters> validator,
        ISpecializationService specializationService,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return new ValidationError(validationResult.ToDictionary());
        }

        var userId = user.GetUserId();
        if (string.IsNullOrWhiteSpace(userId)) 
            return SpecializationErrors.Unauthorized;

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
        ClaimsPrincipal user,
        IValidator<UpdateSpecializationDto> validator,
        ISpecializationService specializationService,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return new ValidationError(validationResult.ToDictionary());
        }

        var userId = user.GetUserId();
        if (string.IsNullOrWhiteSpace(userId)) 
            return SpecializationErrors.Unauthorized;

        var model = dto.Adapt<SpecializationModel>();
        var result = await specializationService.UpdateAsync(id, model, userId, ct);

        return result.Map(m => m.Adapt<SpecializationResponseDto>());
    }
}
