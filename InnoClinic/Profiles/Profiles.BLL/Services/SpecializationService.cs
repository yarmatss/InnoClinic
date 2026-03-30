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

        model.Adapt(existingEntity);

        specializationRepository.MarkUpdate(existingEntity);
        await specializationRepository.SaveChangesAsync(cancellationToken);

        return existingEntity.Adapt<SpecializationModel>();
    }
}
