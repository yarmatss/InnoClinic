using Profiles.BLL.Models;
using Profiles.Domain.Common;
using Profiles.Domain.Models;

namespace Profiles.BLL.Interfaces;

public interface IMedicalStaffService
{
    Task<Result<MedicalStaffModel>> CreateAsync(
        MedicalStaffModel model, 
        CancellationToken cancellationToken);

    Task<Result<MedicalStaffModel>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<Result<PagedResponse<MedicalStaffModel>>> GetPagedAsync(
        MedicalStaffQueryParameters query,
        CancellationToken cancellationToken);

    Task<Result<MedicalStaffModel>> UpdateAsync(
        Guid id,
        MedicalStaffModel model,
        CancellationToken cancellationToken);
    
    Task<Result> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken);
    
    Task<Result> AssignSpecializationsAsync(
        Guid staffId,
        IReadOnlyList<StaffSpecializationModel> assignments,
        CancellationToken cancellationToken);
}
