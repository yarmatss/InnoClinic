using Google.Protobuf;
using InnoClinic.Contracts.Grpc;
using InnoClinic.Core.Authorization;
using InnoClinic.Core.Common;
using InnoClinic.Messaging.Contracts;
using InnoClinic.Messaging.Outbox;
using Mapster;
using Profiles.BLL.Errors;
using Profiles.BLL.Interfaces;
using Profiles.BLL.Models;
using Profiles.DAL.Entities;
using Profiles.DAL.Interfaces;
using Profiles.Domain.Extensions;
using Profiles.Domain.Models;

namespace Profiles.BLL.Services;

internal class MedicalStaffService(
    IMedicalStaffRepository staffRepository,
    ISpecializationRepository specializationRepository,
    IOutboxRepository outboxRepository,
    IAuthManagementService auth0Service,
    INotificationProducer notificationProducer,
    ICurrentUser currentUser,
    IAccessResolver accessResolver) : IMedicalStaffService
{
    public async Task<Result<MedicalStaffModel>> CreateAsync(
        MedicalStaffModel model,
        CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        if (!CanModify(id, Operation.Create))
            return MedicalStaffErrors.Forbidden;

        var validationError = await ValidateUniquenessAsync(model, null, cancellationToken);
        if (validationError is not null)
            return validationError;

        var entity = model.Adapt<MedicalStaff>();
        entity.Id = id;
        entity.IsActive = true;

        var provisionResult = await auth0Service.ProvisionUserAsync(
            model.Email,
            model.FirstName,
            model.LastName,
            model.StaffType.ToUserRole(),
            entity.Id,
            cancellationToken);

        if (provisionResult.IsFailure)
            return provisionResult.Error;

        var (accountId, invitationUrl) = provisionResult.Value;
        entity.UserId = accountId;

        staffRepository.MarkAdd(entity);
        QueueProfileSyncEvent(entity);

        notificationProducer.Enqueue(new StaffCreated(
            entity.Id,
            entity.FirstName,
            entity.LastName,
            entity.Email,
            invitationUrl
        ));

        try
        {
            await staffRepository.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await auth0Service.DeleteUserAsync(accountId, CancellationToken.None);
            throw;
        }

        return entity.Adapt<MedicalStaffModel>();
    }

    public async Task<Result<MedicalStaffModel>> GetCurrentAsync(
        CancellationToken cancellationToken)
    {
        var entity = await staffRepository.GetByIdAsync(currentUser.ProfileId, cancellationToken);
        if (entity is null)
            return MedicalStaffErrors.NotFound;

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

        var model = existingEntity.Adapt<MedicalStaffModel>();
        ApplyFieldVisibility(model);
        return model;
    }

    public async Task<Result<PagedResponse<MedicalStaffModel>>> GetPagedAsync(
        MedicalStaffQueryParameters query,
        CancellationToken cancellationToken)
    {
        var (entities, totalCount) = await staffRepository.GetPagedAsync(
            query,
            cancellationToken);

        var models = entities.Adapt<IReadOnlyList<MedicalStaffModel>>();

        foreach (var model in models)
        {
            ApplyFieldVisibility(model);
        }

        var pagedResult = new PagedResponse<MedicalStaffModel>
        {
            Items = models,
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
        if (!CanModify(id, Operation.Update))
            return MedicalStaffErrors.Forbidden;

        var existingEntity = await staffRepository.GetByIdAsync(
            id,
            cancellationToken,
            trackChanges: true);

        if (existingEntity is null)
            return MedicalStaffErrors.NotFound;

        var validationError = await ValidateUniquenessAsync(model, id, cancellationToken);
        if (validationError is not null)
            return validationError;

        var currentRole = existingEntity.StaffType.ToUserRole();
        var newRole = model.StaffType.ToUserRole();
        var userId = existingEntity.UserId;

        if (currentRole != newRole)
        {
            var roleResult = await auth0Service.ChangeRoleAsync(userId, currentRole, newRole, cancellationToken);
            if (roleResult.IsFailure)
                return roleResult.Error;
        }

        model.Id = id;
        model.Adapt(existingEntity);

        staffRepository.MarkUpdate(existingEntity);
        QueueProfileSyncEvent(existingEntity);

        try
        {
            await staffRepository.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            if (currentRole != newRole)
                await auth0Service.ChangeRoleAsync(
                    userId,
                    currentRole: newRole,
                    newRole: currentRole,
                    CancellationToken.None);
            throw;
        }

        return existingEntity.Adapt<MedicalStaffModel>();
    }

    public async Task<Result> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!CanModify(id, Operation.Delete))
            return MedicalStaffErrors.Forbidden;

        var existingEntity = await staffRepository.GetByIdAsync(
            id,
            cancellationToken,
            trackChanges: true);

        if (existingEntity is null)
            return MedicalStaffErrors.NotFound;

        var userId = existingEntity.UserId;

        var blockResult = await auth0Service.SetBlockedAsync(userId, blocked: true, cancellationToken);
        if (blockResult.IsFailure)
            return blockResult.Error;

        existingEntity.IsActive = false;

        staffRepository.MarkUpdate(existingEntity);
        QueueProfileSyncEvent(existingEntity);

        try
        {
            await staffRepository.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await auth0Service.SetBlockedAsync(userId, blocked: false, CancellationToken.None);
            throw;
        }

        return Result.Success();
    }

    public async Task<Result> AssignSpecializationsAsync(
        Guid staffId,
        IReadOnlyList<StaffSpecializationModel> assignments,
        CancellationToken cancellationToken)
    {
        if (!CanModify(staffId, Operation.Update))
            return MedicalStaffErrors.Forbidden;

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
        if (!CanModify(staffId, Operation.Update))
            return MedicalStaffErrors.Forbidden;

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
        if (!CanModify(staffId, Operation.Update))
            return MedicalStaffErrors.Forbidden;

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
        if (!CanModify(staffId, Operation.Update))
            return MedicalStaffErrors.Forbidden;

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

    private bool CanModify(Guid staffId, Operation operation) =>
        accessResolver.Check(new StaffRef(staffId), operation).IsAllowed;

    private void ApplyFieldVisibility(MedicalStaffModel model)
    {
        if (accessResolver.Check(new StaffRef(model.Id), Operation.Read) == AccessLevel.Full)
            return;

        model.NationalId = string.Empty;
        model.LicenseNumber = string.Empty;
        model.Email = string.Empty;
        model.ContactPhone = string.Empty;
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

        var existingEmail = await staffRepository.GetByConditionAsync(
            s => s.Email == model.Email && (!currentId.HasValue || s.Id != currentId.Value),
            cancellationToken);

        if (existingEmail.Any())
            return MedicalStaffErrors.DuplicateEmail;

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
