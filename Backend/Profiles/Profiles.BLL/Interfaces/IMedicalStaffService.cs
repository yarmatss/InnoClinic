using Profiles.BLL.Models;
using InnoClinic.Core.Common;
using Profiles.Domain.Models;

namespace Profiles.BLL.Interfaces;

public interface IMedicalStaffService
{
    Task<Result<MedicalStaffModel>> CreateAsync(
        MedicalStaffModel model, 
        string userId,
        CancellationToken cancellationToken);

    Task<Result<MedicalStaffModel>> GetByIdAsync(
        Guid id,
        string userId,
        CancellationToken cancellationToken);

    Task<Result<PagedResponse<MedicalStaffModel>>> GetPagedAsync(
        MedicalStaffQueryParameters query,
        string userId,
        CancellationToken cancellationToken);

    Task<Result<MedicalStaffModel>> UpdateAsync(
        Guid id,
        MedicalStaffModel model,
        string userId,
        CancellationToken cancellationToken);
    
    Task<Result> DeactivateAsync(
        Guid id,
        string userId,
        CancellationToken cancellationToken);
    
    Task<Result> AssignSpecializationsAsync(
        Guid staffId,
        IReadOnlyList<StaffSpecializationModel> assignments,
        string userId,
        CancellationToken cancellationToken);

    Task<Result> SetWorkingHoursAsync(
        Guid staffId,
        IReadOnlyList<WorkingHoursModel> workingHoursModels,
        string userId,
        CancellationToken cancellationToken);

    Task<Result> SetScheduleOverridesAsync(
        Guid staffId,
        IReadOnlyList<ScheduleOverrideModel> overrideModels,
        string userId,
        CancellationToken cancellationToken);

    Task<Result> DeleteScheduleOverrideAsync(
        Guid staffId,
        DateOnly date,
        string userId,
        CancellationToken cancellationToken);

    Task<Result<MedicalStaffModel>> GetCurrentAsync(
        string userId,
        string? email,
        CancellationToken cancellationToken);
}
