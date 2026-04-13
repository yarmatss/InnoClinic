using Profiles.DAL.Entities;

namespace Profiles.DAL.Interfaces;

public interface IPatientRepository : IBaseRepository<Patient>
{
    Task<(IReadOnlyList<Patient> Items, int TotalCount)> GetPagedAsync(
        string? firstName,
        string? lastName,
        DateOnly? birthDate,
        string? sortBy,
        bool isDescending,
        int pageNumber,
        int pageSize,
        CancellationToken ct);
}
