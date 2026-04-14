using Profiles.DAL.Entities;
using Profiles.Domain.Models;

namespace Profiles.DAL.Interfaces;

public interface ISpecializationRepository : IBaseRepository<Specialization>
{
    Task<(IReadOnlyList<Specialization> Items, int TotalCount)> GetPagedAsync(
        SpecializationQueryParameters parameters,
        CancellationToken ct);
}
