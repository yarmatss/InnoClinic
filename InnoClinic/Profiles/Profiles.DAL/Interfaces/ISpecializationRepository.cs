using Profiles.DAL.Entities;

namespace Profiles.DAL.Interfaces;

public interface ISpecializationRepository : IBaseRepository<Specialization>
{
    Task<(IReadOnlyList<Specialization> Items, int TotalCount)> GetPagedAsync(
        string? name,
        string? sortBy,
        bool isDescending,
        int pageNumber,
        int pageSize,
        CancellationToken ct);
}
