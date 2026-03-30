using Mapster;
using Profiles.BLL.Errors;
using Profiles.BLL.Interfaces;
using Profiles.BLL.Models;
using Profiles.DAL.Entities;
using Profiles.DAL.Interfaces;
using Profiles.Domain.Common;

namespace Profiles.BLL.Services;

internal class MedicalStaffService(IMedicalStaffRepository staffRepository) : IMedicalStaffService
{
    public async Task<Result<MedicalStaffModel>> CreateAsync(
        MedicalStaffModel model, 
        CancellationToken cancellationToken)
    {
        var entity = model.Adapt<MedicalStaff>();
        entity.IsActive = true;

        staffRepository.MarkAdd(entity);
        await staffRepository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<MedicalStaffModel>();
    }

    public async Task<Result<IReadOnlyList<MedicalStaffModel>>> GetAllActiveAsync(
        CancellationToken cancellationToken)
    {
        var activeEntities = await staffRepository.GetByConditionAsync(
            s => s.IsActive,
            cancellationToken);
        
        return Result.Success(activeEntities.Adapt<IReadOnlyList<MedicalStaffModel>>());
    }

    public async Task<Result<MedicalStaffModel>> UpdateAsync(
        Guid id, 
        MedicalStaffModel model, 
        CancellationToken cancellationToken)
    {
        var existingEntity = await staffRepository.GetByIdAsync(
            id,
            cancellationToken,
            trackChanges: true);

        if (existingEntity is null)
            return MedicalStaffErrors.NotFound;

        model.Adapt(existingEntity);

        staffRepository.MarkUpdate(existingEntity);
        await staffRepository.SaveChangesAsync(cancellationToken);

        return existingEntity.Adapt<MedicalStaffModel>();
    }

    public async Task<Result> DeactivateAsync(Guid id, CancellationToken cancellationToken)
    {
        var existingEntity = await staffRepository.GetByIdAsync(
            id,
            cancellationToken,
            trackChanges: true);

        if (existingEntity is null)
            return MedicalStaffErrors.NotFound;

        existingEntity.IsActive = false;

        staffRepository.MarkUpdate(existingEntity);
        await staffRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> AssignSpecializationsAsync(
        Guid staffId,
        IReadOnlyList<StaffSpecializationModel> assignments,
        CancellationToken cancellationToken)
    {
        var existingEntity = await staffRepository.GetByIdAsync(
            staffId,
            cancellationToken,
            trackChanges: true);

        if (existingEntity is null)
            return MedicalStaffErrors.NotFound;

        existingEntity.StaffSpecializations = assignments.Adapt<List<StaffSpecialization>>();

        staffRepository.MarkUpdate(existingEntity);
        await staffRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
