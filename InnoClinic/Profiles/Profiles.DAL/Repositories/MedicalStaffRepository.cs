using Microsoft.EntityFrameworkCore;
using Profiles.DAL.Data;
using Profiles.DAL.Entities;
using Profiles.DAL.Extensions;
using Profiles.DAL.Interfaces;
using Profiles.Domain.Enums;
using System.Linq.Expressions;

namespace Profiles.DAL.Repositories;

public class MedicalStaffRepository(ProfilesDbContext context) : 
    BaseRepository<MedicalStaff>(context), IMedicalStaffRepository
{
    public override async Task<IReadOnlyList<MedicalStaff>> GetByConditionAsync(
        Expression<Func<MedicalStaff, bool>> expression, 
        CancellationToken ct, 
        bool trackChanges = false)
    {
        return await GetQuery(trackChanges)
            .Include(x => x.StaffSpecializations)
                .ThenInclude(ss => ss.Specialization)
            .Where(expression)
            .ToListAsync(ct);
    }

    public override async Task<MedicalStaff?> GetByIdAsync(
        Guid id, 
        CancellationToken ct, 
        bool trackChanges = false)
    {
        return await GetQuery(trackChanges)
            .Include(x => x.StaffSpecializations)
                .ThenInclude(ss => ss.Specialization)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<(IReadOnlyList<MedicalStaff> Items, int TotalCount)> GetPagedAsync(
        string? firstName,
        string? lastName,
        StaffType? staffType,
        Guid? specializationId,
        string? sortBy,
        bool isDescending,
        int pageNumber,
        int pageSize,
        CancellationToken ct)
    {
        var query = GetQuery(trackChanges: false);

        if (!string.IsNullOrWhiteSpace(firstName))
            query = query.Where(m => EF.Functions.ILike(m.FirstName, $"%{firstName}%"));

        if (!string.IsNullOrWhiteSpace(lastName))
            query = query.Where(m => EF.Functions.ILike(m.LastName, $"%{lastName}%"));

        if (staffType.HasValue)
            query = query.Where(m => m.StaffType == staffType.Value);

        if (specializationId.HasValue)
            query = query.Where(m => m.StaffSpecializations.Any(ss => ss.SpecializationId == specializationId.Value));

        var totalCount = await query.CountAsync(ct);

        query = sortBy?.ToLower() switch
        {
            "firstname" => isDescending ? query.OrderByDescending(m => m.FirstName) : query.OrderBy(m => m.FirstName),
            "lastname" => isDescending ? query.OrderByDescending(m => m.LastName) : query.OrderBy(m => m.LastName),
            "stafftype" => isDescending ? query.OrderByDescending(m => m.StaffType) : query.OrderBy(m => m.StaffType),
            _ => query.OrderBy(m => m.Id)
        };

        var items = await query
            .Include(x => x.StaffSpecializations)
                .ThenInclude(ss => ss.Specialization)
            .ApplyPagination(pageNumber, pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
