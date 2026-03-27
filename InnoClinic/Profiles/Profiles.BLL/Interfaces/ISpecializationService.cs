using Profiles.BLL.Models;
using Profiles.Domain.Common;

namespace Profiles.BLL.Interfaces;

public interface ISpecializationService
{
    Task<Result<SpecializationModel>> CreateAsync(
        SpecializationModel model, 
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<SpecializationModel>>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<Result<SpecializationModel>> UpdateAsync(
        Guid id,
        SpecializationModel model,
        CancellationToken cancellationToken);
}
