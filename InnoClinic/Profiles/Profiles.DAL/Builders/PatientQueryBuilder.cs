using Microsoft.EntityFrameworkCore;
using Profiles.DAL.Entities;

namespace Profiles.DAL.Builders;

internal class PatientQueryBuilder(IQueryable<Patient> query)
{
    public PatientQueryBuilder FilterByFirstName(string? firstName)
    {
        if (!string.IsNullOrWhiteSpace(firstName))
            query = query.Where(p => EF.Functions.ILike(p.FirstName, $"%{firstName}%"));
        return this;
    }

    public PatientQueryBuilder FilterByLastName(string? lastName)
    {
        if (!string.IsNullOrWhiteSpace(lastName))
            query = query.Where(p => EF.Functions.ILike(p.LastName, $"%{lastName}%"));
        return this;
    }

    public PatientQueryBuilder FilterByBirthDate(DateOnly? birthDate)
    {
        if (birthDate.HasValue)
            query = query.Where(p => p.BirthDate == birthDate.Value);
        return this;
    }

    public PatientQueryBuilder SortBy(string? sortBy, bool isDescending)
    {
        query = sortBy?.ToLower() switch
        {
            "firstname" => isDescending ? query.OrderByDescending(p => p.FirstName) : query.OrderBy(p => p.FirstName),
            "lastname" => isDescending ? query.OrderByDescending(p => p.LastName) : query.OrderBy(p => p.LastName),
            "birthdate" => isDescending ? query.OrderByDescending(p => p.BirthDate) : query.OrderBy(p => p.BirthDate),
            _ => query.OrderBy(p => p.Id)
        };
        return this;
    }

    public IQueryable<Patient> Build() => query;
}
