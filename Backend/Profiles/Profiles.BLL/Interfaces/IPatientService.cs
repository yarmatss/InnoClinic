using Profiles.BLL.Models;
using InnoClinic.Core.Common;
using Profiles.Domain.Models;

namespace Profiles.BLL.Interfaces;

public interface IPatientService
{
    Task<Result<PatientModel>> CreateAsync(
        PatientModel model, 
        string userId,
        CancellationToken cancellationToken);

    Task<Result<PagedResponse<PatientModel>>> GetAllAsync(
        PatientQueryParameters queryModel,
        string userId,
        CancellationToken cancellationToken);

    Task<Result<PatientModel>> GetByIdAsync(
        Guid id, 
        string userId,
        CancellationToken cancellationToken);

    Task<Result<PatientModel>> UpdateAsync(
        Guid id, 
        PatientModel model, 
        string userId,
        CancellationToken cancellationToken);

    Task<Result<PatientModel>> GetCurrentAsync(
        string userId,
        string? email,
        CancellationToken cancellationToken);
}
