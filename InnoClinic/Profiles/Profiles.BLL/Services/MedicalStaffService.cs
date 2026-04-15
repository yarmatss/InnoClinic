using Mapster;
using Profiles.BLL.Errors;
using Profiles.BLL.Interfaces;
using Profiles.BLL.Models;
using Profiles.DAL.Entities;
using Profiles.DAL.Interfaces;
using Profiles.Domain.Models;
using Profiles.Domain.Common;

namespace Profiles.BLL.Services;

internal class MedicalStaffService(
    IMedicalStaffRepository staffRepository,
    ISpecializationRepository specializationRepository) : IMedicalStaffService
{
    public async Task<Result<MedicalStaffModel>> CreateAsync(
        MedicalStaffModel model, 
        CancellationToken cancellationToken)
    {
        var validationError = await ValidateUniquenessAsync(model, null, cancellationToken);
        if (validationError is not null)
            return validationError;

        var entity = model.Adapt<MedicalStaff>();
        entity.IsActive = true;

        staffRepository.MarkAdd(entity);
        await staffRepository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<MedicalStaffModel>();
    }

    public async Task<Result<MedicalStaffModel>> GetByIdAsync(
        Guid id, 
        CancellationToken cancellationToken)
    {
        var existingEntity = await staffRepository.GetByIdAsync(
            id,
            cancellationToken,
            trackChanges: false);

        if (existingEntity is null)
            return MedicalStaffErrors.NotFound;

        return existingEntity.Adapt<MedicalStaffModel>();
    }

    public async Task<Result<PagedResponse<MedicalStaffModel>>> GetPagedAsync(
        MedicalStaffQueryParameters query,
        CancellationToken ct)
    {
        var (entities, totalCount) = await staffRepository.GetPagedAsync(
            query,
            ct);

        var pagedResult = new PagedResponse<MedicalStaffModel>
        {
            Items = entities.Adapt<IReadOnlyList<MedicalStaffModel>>(),
            TotalCount = totalCount,
            PageNumber = query.PageNumber!.Value,
            PageSize = query.PageSize!.Value
        };

        return pagedResult;
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

        var validationError = await ValidateUniquenessAsync(model, id, cancellationToken);
        if (validationError is not null)
            return validationError;

        model.Id = id;
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

        if (assignments.Any())
        {
            var requestedIds = assignments.Select(a => a.SpecializationId).ToList();
            var existingSpecs = await specializationRepository.GetByConditionAsync(
                s => requestedIds.Contains(s.Id), cancellationToken);

            if (existingSpecs.Count != requestedIds.Count)
                return MedicalStaffErrors.InvalidSpecialization;
        }

        existingEntity.StaffSpecializations.Clear();
        await staffRepository.SaveChangesAsync(cancellationToken);

        var newSpecializations = assignments.Adapt<List<StaffSpecialization>>();
        foreach (var spec in newSpecializations)
        {
            existingEntity.StaffSpecializations.Add(spec);
        }

        await staffRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Error?> ValidateUniquenessAsync(
        MedicalStaffModel model,
        Guid? currentId,
        CancellationToken cancellationToken)
    {
        var existingLicense = await staffRepository.GetByConditionAsync(
            s => s.LicenseNumber == model.LicenseNumber && (!currentId.HasValue || s.Id != currentId.Value), 
            cancellationToken);

        if (existingLicense.Any())
            return MedicalStaffErrors.DuplicateLicenseNumber;

        var existingNationalId = await staffRepository.GetByConditionAsync(
            s => s.NationalId == model.NationalId && (!currentId.HasValue || s.Id != currentId.Value), 
            cancellationToken);

        if (existingNationalId.Any())
            return MedicalStaffErrors.DuplicateNationalId;

        if (model.SupervisorId.HasValue)
        {
            var supervisor = await staffRepository.GetByIdAsync(model.SupervisorId.Value, cancellationToken);
            if (supervisor is null || !supervisor.IsActive)
                return MedicalStaffErrors.SupervisorNotFound;
        }

        return null;
    }
}
