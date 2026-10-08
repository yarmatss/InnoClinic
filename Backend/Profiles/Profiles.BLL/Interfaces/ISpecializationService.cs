using Profiles.BLL.Models;
using InnoClinic.Core.Common;
using Profiles.Domain.Models;

namespace Profiles.BLL.Interfaces;

public interface ISpecializationService
{
    Task<Result<SpecializationModel>> CreateAsync(
        SpecializationModel model, 
        string userId,
        CancellationToken cancellationToken);

    Task<Result<PagedResponse<SpecializationModel>>> GetPagedAsync(
        SpecializationQueryParameters query,
        CancellationToken ct);

    Task<Result<SpecializationModel>> UpdateAsync(
        Guid id,
        SpecializationModel model,
        string userId,
        CancellationToken cancellationToken);
}
