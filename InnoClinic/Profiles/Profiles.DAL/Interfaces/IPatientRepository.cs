using Profiles.DAL.Entities;
using Profiles.Domain.Models;

namespace Profiles.DAL.Interfaces;

public interface IPatientRepository : IBaseRepository<Patient>
{
    Task<(IReadOnlyList<Patient> Items, int TotalCount)> GetPagedAsync(
        PatientQueryParameters parameters,
        CancellationToken ct);
}
