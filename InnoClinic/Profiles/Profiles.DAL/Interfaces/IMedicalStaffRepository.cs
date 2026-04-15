using Profiles.DAL.Entities;
using Profiles.Domain.Models;

namespace Profiles.DAL.Interfaces;

public interface IMedicalStaffRepository : IBaseRepository<MedicalStaff>
{
    Task<(IReadOnlyList<MedicalStaff> Items, int TotalCount)> GetPagedAsync(
        MedicalStaffQueryParameters parameters,
        CancellationToken ct);
}
