using Google.Protobuf;
using InnoClinic.Contracts.Grpc;
using InnoClinic.Core.Common;
using Mapster;
using Profiles.BLL.Errors;
using Profiles.BLL.Interfaces;
using Profiles.BLL.Models;
using Profiles.DAL.Entities;
using Profiles.DAL.Interfaces;
using Profiles.Domain.Models;

namespace Profiles.BLL.Services;

internal class MedicalStaffService(
    IMedicalStaffRepository staffRepository,
    ISpecializationRepository specializationRepository,
    IOutboxRepository outboxRepository) : IMedicalStaffService
{
    public async Task<Result<MedicalStaffModel>> CreateAsync(
        MedicalStaffModel model, 
        CancellationToken cancellationToken)
    {
        var validationError = await ValidateUniquenessAsync(model, null, cancellationToken);
        if (validationError is not null)
            return validationError;

        var entity = model.Adapt<MedicalStaff>();
        entity.Id = Guid.NewGuid();
        entity.IsActive = true;

        staffRepository.MarkAdd(entity);
        QueueProfileSyncEvent(entity);
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
        CancellationToken cancellationToken)
    {
        var (entities, totalCount) = await staffRepository.GetPagedAsync(
            query,
            cancellationToken);

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
        QueueProfileSyncEvent(existingEntity);
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
        QueueProfileSyncEvent(existingEntity);
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

        QueueProfileSyncEvent(existingEntity);
        await staffRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> SetWorkingHoursAsync(
        Guid staffId,
        IReadOnlyList<WorkingHoursModel> workingHoursModels,
        CancellationToken cancellationToken)
    {
        var existingEntity = await staffRepository.GetByIdAsync(
            staffId,
            cancellationToken,
            trackChanges: true);

        if (existingEntity is null)
            return MedicalStaffErrors.NotFound;

        foreach (var workingHours in workingHoursModels)
        {
            if (!workingHours.IsDayOff && workingHours.StartTime >= workingHours.EndTime)
                return MedicalStaffErrors.InvalidWorkingHours;
        }

        var existingWorkingHoursDict = existingEntity.WorkingHours
            .ToDictionary(wh => wh.DayOfWeek);

        var newWorkingHours = workingHoursModels.Select(wh => new WorkingHours
        {
            MedicalStaffId = staffId,
            DayOfWeek = wh.DayOfWeek,
            StartTime = wh.StartTime,
            EndTime = wh.EndTime,
            IsDayOff = wh.IsDayOff
        }).ToList();

        foreach (var newWh in newWorkingHours)
        {
            if (existingWorkingHoursDict.TryGetValue(newWh.DayOfWeek, out var existingWh))
            {
                existingWh.StartTime = newWh.StartTime;
                existingWh.EndTime = newWh.EndTime;
                existingWh.IsDayOff = newWh.IsDayOff;

                existingWorkingHoursDict.Remove(newWh.DayOfWeek);
            }
            else
            {
                existingEntity.WorkingHours.Add(newWh);
            }
        }

        foreach (var leftOver in existingWorkingHoursDict.Values)
        {
            existingEntity.WorkingHours.Remove(leftOver);
        }

        QueueProfileSyncEvent(existingEntity);
        await staffRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> SetScheduleOverridesAsync(
        Guid staffId,
        IReadOnlyList<ScheduleOverrideModel> overrideModels,
        CancellationToken cancellationToken)
    {
        var existingEntity = await staffRepository.GetByIdAsync(
            staffId,
            cancellationToken,
            trackChanges: true);

        if (existingEntity is null)
            return MedicalStaffErrors.NotFound;

        foreach (var over in overrideModels)
        {
            if (!over.IsDayOff && (over.StartTime >= over.EndTime || !over.StartTime.HasValue || !over.EndTime.HasValue))
                return MedicalStaffErrors.InvalidWorkingHours;
        }

        var newOverrides = overrideModels.Select(o => new ScheduleOverride
        {
            MedicalStaffId = staffId,
            Date = o.Date,
            StartTime = o.StartTime,
            EndTime = o.EndTime,
            IsDayOff = o.IsDayOff
        }).ToList();

        var existingOverridesDict = existingEntity.ScheduleOverrides
            .ToDictionary(o => o.Date);

        foreach (var newOver in newOverrides)
        {
            if (existingOverridesDict.TryGetValue(newOver.Date, out var existingOverride))
            {
                existingOverride.StartTime = newOver.StartTime;
                existingOverride.EndTime = newOver.EndTime;
                existingOverride.IsDayOff = newOver.IsDayOff;
            }
            else
            {
                existingEntity.ScheduleOverrides.Add(newOver);
            }
        }

        QueueProfileSyncEvent(existingEntity);
        await staffRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteScheduleOverrideAsync(
        Guid staffId,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var existingEntity = await staffRepository.GetByIdAsync(
            staffId,
            cancellationToken,
            trackChanges: true);

        if (existingEntity is null)
            return MedicalStaffErrors.NotFound;

        var existingOverride = existingEntity.ScheduleOverrides.FirstOrDefault(o => o.Date == date);
        if (existingOverride is null)
            return MedicalStaffErrors.OverrideNotFound;

        existingEntity.ScheduleOverrides.Remove(existingOverride);

        QueueProfileSyncEvent(existingEntity);
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

    private void QueueProfileSyncEvent(MedicalStaff staffEntity)
    {
        var syncRequest = staffEntity.Adapt<SyncStaffProfileRequest>();

        string jsonPayload = JsonFormatter.Default.Format(syncRequest);

        var outboxMessage = new OutboxMessage
        {
            Type = nameof(SyncStaffProfileRequest),
            Content = jsonPayload,
            OccurredOnUtc = DateTime.UtcNow
        };

        outboxRepository.MarkAdd(outboxMessage);
    }
}
