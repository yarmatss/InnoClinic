using Profiles.BLL.Models;
using Profiles.Domain.Common;
using Profiles.Domain.Models;

namespace Profiles.BLL.Interfaces;

public interface IPatientService
{
    Task<Result<PatientModel>> CreateAsync(
        PatientModel model, 
        CancellationToken cancellationToken);

    Task<Result<PagedResponse<PatientModel>>> GetAllAsync(
        PatientQueryParameters queryModel,
        CancellationToken cancellationToken);

    Task<Result<PatientModel>> GetByIdAsync(
        Guid id, 
        CancellationToken cancellationToken);

    Task<Result<PatientModel>> UpdateAsync(
        Guid id, 
        PatientModel model, 
        CancellationToken cancellationToken);
}
