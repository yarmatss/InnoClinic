using Profiles.BLL.Models;
using Profiles.Domain.Common;
using Profiles.Domain.Models;

namespace Profiles.BLL.Interfaces;

public interface ISpecializationService
{
    Task<Result<SpecializationModel>> CreateAsync(
        SpecializationModel model, 
        CancellationToken cancellationToken);

    Task<Result<PagedResponse<SpecializationModel>>> GetPagedAsync(
        SpecializationQueryParameters query,
        CancellationToken ct);

    Task<Result<SpecializationModel>> UpdateAsync(
        Guid id,
        SpecializationModel model,
        CancellationToken cancellationToken);
}
