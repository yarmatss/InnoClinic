using Microsoft.EntityFrameworkCore;
using Profiles.DAL.Builders;
using Profiles.DAL.Data;
using Profiles.DAL.Entities;
using Profiles.DAL.Extensions;
using Profiles.DAL.Interfaces;
using Profiles.Domain.Models;
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
        MedicalStaffQueryParameters parameters,
        CancellationToken ct)
    {
        var builder = new MedicalStaffQueryBuilder(GetQuery(trackChanges: false))
            .FilterByFirstName(parameters.FirstName)
            .FilterByLastName(parameters.LastName)
            .FilterByStaffType(parameters.StaffType)
            .FilterBySpecializationId(parameters.SpecializationId);

        var query = builder.Build();
        var totalCount = await query.CountAsync(ct);

        var items = await builder
            .SortBy(parameters.SortBy, parameters.IsDescending)
            .IncludeSpecializations()
            .Build()
            .ApplyPagination(parameters.PageNumber!.Value, parameters.PageSize!.Value)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
