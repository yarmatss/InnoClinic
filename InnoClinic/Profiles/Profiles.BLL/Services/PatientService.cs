using Mapster;
using Profiles.BLL.Errors;
using Profiles.BLL.Interfaces;
using Profiles.BLL.Models;
using Profiles.DAL.Entities;
using Profiles.DAL.Interfaces;
using Profiles.Domain.Common;

namespace Profiles.BLL.Services;

internal class PatientService(IPatientRepository patientRepository) : IPatientService
{
    public async Task<Result<PatientModel>> CreateAsync(
        PatientModel model, 
        CancellationToken cancellationToken)
    {
        var entity = model.Adapt<Patient>();

        patientRepository.MarkAdd(entity);
        await patientRepository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<PatientModel>();
    }

    public async Task<Result<IReadOnlyList<PatientModel>>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var entities = await patientRepository.GetAllAsync(cancellationToken);

        var models = entities.Adapt<IReadOnlyList<PatientModel>>();

        return Result.Success(models);
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

        model.Id = id;
        model.Adapt(existingEntity);

        patientRepository.MarkUpdate(existingEntity);
        await patientRepository.SaveChangesAsync(cancellationToken);

        return existingEntity.Adapt<PatientModel>();
    }
}
