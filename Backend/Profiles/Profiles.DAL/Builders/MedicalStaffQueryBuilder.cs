using Microsoft.EntityFrameworkCore;
using Profiles.DAL.Entities;
using Profiles.Domain.Enums;

namespace Profiles.DAL.Builders;

internal class MedicalStaffQueryBuilder(IQueryable<MedicalStaff> query)
{
    public MedicalStaffQueryBuilder FilterByFirstName(string? firstName)
    {
        if (!string.IsNullOrWhiteSpace(firstName))
            query = query.Where(m => EF.Functions.ILike(m.FirstName, $"%{firstName}%"));
        return this;
    }

    public MedicalStaffQueryBuilder FilterByLastName(string? lastName)
    {
        if (!string.IsNullOrWhiteSpace(lastName))
            query = query.Where(m => EF.Functions.ILike(m.LastName, $"%{lastName}%"));
        return this;
    }

    public MedicalStaffQueryBuilder FilterByStaffType(StaffType? staffType)
    {
        if (staffType.HasValue)
            query = query.Where(m => m.StaffType == staffType.Value);
        return this;
    }

    public MedicalStaffQueryBuilder FilterBySpecializationId(Guid? specializationId)
    {
        if (specializationId.HasValue)
            query = query.Where(m => m.StaffSpecializations.Any(ss => ss.SpecializationId == specializationId.Value));
        return this;
    }

    public MedicalStaffQueryBuilder SortBy(string? sortBy, bool isDescending)
    {
        query = sortBy?.ToLower() switch
        {
            "firstname" => isDescending ? query.OrderByDescending(m => m.FirstName) : query.OrderBy(m => m.FirstName),
            "lastname" => isDescending ? query.OrderByDescending(m => m.LastName) : query.OrderBy(m => m.LastName),
            "stafftype" => isDescending ? query.OrderByDescending(m => m.StaffType) : query.OrderBy(m => m.StaffType),
            _ => query.OrderBy(m => m.Id)
        };
        return this;
    }

    public MedicalStaffQueryBuilder IncludeSpecializations()
    {
        query = query.Include(x => x.StaffSpecializations).ThenInclude(ss => ss.Specialization);
        return this;
    }

    public IQueryable<MedicalStaff> Build() => query;
}
