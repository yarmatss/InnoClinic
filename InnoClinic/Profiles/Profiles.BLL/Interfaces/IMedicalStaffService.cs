using Profiles.BLL.Models;
using Profiles.Domain.Common;

namespace Profiles.BLL.Interfaces;

public interface IMedicalStaffService
{
    Task<Result<MedicalStaffModel>> CreateAsync(
        MedicalStaffModel model, 
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<MedicalStaffModel>>> GetAllActiveAsync(CancellationToken cancellationToken);
    
    Task<Result<MedicalStaffModel>> UpdateAsync(
        Guid id,
        MedicalStaffModel model,
        CancellationToken cancellationToken);
    
    Task<Result> DeactivateAsync(Guid id, CancellationToken cancellationToken);
    
    Task<Result> AssignSpecializationsAsync(
        Guid staffId,
        IReadOnlyList<StaffSpecializationModel> assignments,
        CancellationToken cancellationToken);
}
