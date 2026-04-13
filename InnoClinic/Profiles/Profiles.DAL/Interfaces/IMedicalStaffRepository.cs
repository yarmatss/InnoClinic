using Profiles.DAL.Entities;
using Profiles.Domain.Enums;

namespace Profiles.DAL.Interfaces;

public interface IMedicalStaffRepository : IBaseRepository<MedicalStaff>
{
    Task<(IReadOnlyList<MedicalStaff> Items, int TotalCount)> GetPagedAsync(
        string? firstName,
        string? lastName,
        StaffType? staffType,
        Guid? specializationId,
        string? sortBy,
        bool isDescending,
        int pageNumber,
        int pageSize,
        CancellationToken ct);
}
