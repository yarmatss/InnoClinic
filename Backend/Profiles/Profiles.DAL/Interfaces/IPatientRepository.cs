using Profiles.DAL.Entities;
using Profiles.Domain.Models;

namespace Profiles.DAL.Interfaces;

public interface IPatientRepository : IBaseRepository<Patient>
{
    Task<(IReadOnlyList<Patient> Items, int TotalCount)> GetPagedAsync(
        PatientQueryParameters parameters,
        CancellationToken ct);

    Task<Patient?> GetByUserIdAsync(
        string userId,
        CancellationToken ct,
        bool trackChanges = false);

    Task<Patient?> GetByEmailAsync(
        string email,
        CancellationToken ct,
        bool trackChanges = false);
}
