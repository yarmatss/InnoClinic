using Profiles.DAL.Entities;
using Profiles.Domain.Models;

namespace Profiles.DAL.Interfaces;

public interface IMedicalStaffRepository : IBaseRepository<MedicalStaff>
{
    Task<(IReadOnlyList<MedicalStaff> Items, int TotalCount)> GetPagedAsync(
        MedicalStaffQueryParameters parameters,
        CancellationToken ct);

    Task<MedicalStaff?> GetByUserIdAsync(
        string userId,
        CancellationToken ct,
        bool trackChanges = false);

    Task<MedicalStaff?> GetByEmailAsync(
        string email,
        CancellationToken ct,
        bool trackChanges = false);
}
