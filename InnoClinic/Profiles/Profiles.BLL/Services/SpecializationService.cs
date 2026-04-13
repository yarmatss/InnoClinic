using Mapster;
using Profiles.BLL.Errors;
using Profiles.BLL.Interfaces;
using Profiles.BLL.Models;
using Profiles.DAL.Entities;
using Profiles.DAL.Interfaces;
using Profiles.Domain.Common;

namespace Profiles.BLL.Services;

internal class SpecializationService(ISpecializationRepository specializationRepository) : ISpecializationService
{
    public async Task<Result<SpecializationModel>> CreateAsync(
        SpecializationModel model,
        CancellationToken cancellationToken)
    {
        var validationError = await ValidateUniquenessAsync(model, null, cancellationToken);
        if (validationError is not null)
            return validationError;

        var entity = model.Adapt<Specialization>();

        specializationRepository.MarkAdd(entity);
        await specializationRepository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<SpecializationModel>();
    }

    public async Task<Result<IReadOnlyList<SpecializationModel>>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var entities = await specializationRepository.GetAllAsync(cancellationToken);

        var models = entities.Adapt<IReadOnlyList<SpecializationModel>>();

        return Result.Success(models);
    }

    public async Task<Result<SpecializationModel>> UpdateAsync(
        Guid id,
        SpecializationModel model,
        CancellationToken cancellationToken)
    {
        var existingEntity = await specializationRepository.GetByIdAsync(
            id,
            cancellationToken,
            trackChanges: true);

        if (existingEntity is null)
            return SpecializationErrors.NotFound;

        var validationError = await ValidateUniquenessAsync(model, id, cancellationToken);
        if (validationError is not null)
            return validationError;

        model.Id = id;
        model.Adapt(existingEntity);

        specializationRepository.MarkUpdate(existingEntity);
        await specializationRepository.SaveChangesAsync(cancellationToken);

        return existingEntity.Adapt<SpecializationModel>();
    }

    private async Task<Error?> ValidateUniquenessAsync(
        SpecializationModel model,
        Guid? currentId,
        CancellationToken cancellationToken)
    {
        var existingName = await specializationRepository.GetByConditionAsync(
            s => s.Name == model.Name && (!currentId.HasValue || s.Id != currentId.Value), 
            cancellationToken);

        if (existingName.Any())
            return SpecializationErrors.DuplicateName;

        return null;
    }
}
