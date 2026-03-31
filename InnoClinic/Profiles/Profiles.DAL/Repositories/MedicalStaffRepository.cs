using Microsoft.EntityFrameworkCore;
using Profiles.DAL.Data;
using Profiles.DAL.Entities;
using Profiles.DAL.Interfaces;
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
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }
}
