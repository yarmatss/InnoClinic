using Mapster;
using Profiles.BLL.Errors;
using Profiles.BLL.Interfaces;
using Profiles.BLL.Models;
using Profiles.DAL.Entities;
using Profiles.DAL.Interfaces;
using Profiles.Domain.Models;
using Profiles.Domain.Common;

namespace Profiles.BLL.Services;

internal class PatientService(
    IPatientRepository patientRepository,
    IMedicalStaffRepository staffRepository) : IPatientService
{
    public async Task<Result<PatientModel>> CreateAsync(
        PatientModel model, 
        CancellationToken cancellationToken)
    {
        var validationError = await ValidateUniquenessAsync(model, null, cancellationToken);
        if (validationError is not null)
            return validationError;

        var entity = model.Adapt<Patient>();

        patientRepository.MarkAdd(entity);
        await patientRepository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<PatientModel>();
    }

    public async Task<Result<PagedResponse<PatientModel>>> GetAllAsync(
        PatientQueryParameters queryModel,
        CancellationToken cancellationToken)
    {
        var (entities, totalCount) = await patientRepository.GetPagedAsync(
            queryModel,
            cancellationToken);

        var pagedResult = new PagedResponse<PatientModel>
        {
            Items = entities.Adapt<IReadOnlyList<PatientModel>>(),
            TotalCount = totalCount,
            PageNumber = queryModel.PageNumber!.Value,
            PageSize = queryModel.PageSize!.Value
        };

        return pagedResult;
    }

    public async Task<Result<PatientModel>> GetByIdAsync(
        Guid id, 
        CancellationToken cancellationToken)
    {
        var entity = await patientRepository.GetByIdAsync(id, cancellationToken);

        if (entity is null) 
            return PatientErrors.NotFound;

        return entity.Adapt<PatientModel>();
    }

    public async Task<Result<PatientModel>> UpdateAsync(
        Guid id, 
        PatientModel model, 
        CancellationToken cancellationToken)
    {
        var existingEntity = await patientRepository.GetByIdAsync(id, cancellationToken, trackChanges: true);
        if (existingEntity is null)
            return PatientErrors.NotFound;

        var validationError = await ValidateUniquenessAsync(model, id, cancellationToken);
        if (validationError is not null)
            return validationError;

        model.Id = id;
        model.Adapt(existingEntity);

        patientRepository.MarkUpdate(existingEntity);
        await patientRepository.SaveChangesAsync(cancellationToken);

        return existingEntity.Adapt<PatientModel>();
    }

    private async Task<Error?> ValidateUniquenessAsync(
        PatientModel model,
        Guid? currentId,
        CancellationToken cancellationToken)
    {
        var existingInsurance = await patientRepository.GetByConditionAsync(
            p => p.InsuranceNumber == model.InsuranceNumber && (!currentId.HasValue || p.Id != currentId.Value), 
            cancellationToken);

        if (existingInsurance.Any())
            return PatientErrors.DuplicateInsuranceNumber;

        var existingNationalId = await patientRepository.GetByConditionAsync(
            p => p.NationalId == model.NationalId && (!currentId.HasValue || p.Id != currentId.Value), 
            cancellationToken);

        if (existingNationalId.Any())
            return PatientErrors.DuplicateNationalId;

        if (model.PrimaryDoctorId.HasValue)
        {
            var doctor = await staffRepository.GetByIdAsync(model.PrimaryDoctorId.Value, cancellationToken);
            if (doctor is null || !doctor.IsActive)
                return PatientErrors.PrimaryDoctorNotFound;
        }

        return null;
    }
}
