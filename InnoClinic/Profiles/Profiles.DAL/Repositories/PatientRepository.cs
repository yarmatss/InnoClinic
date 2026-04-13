using Profiles.DAL.Data;
using Profiles.DAL.Entities;
using Profiles.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Profiles.DAL.Extensions;

namespace Profiles.DAL.Repositories;

public class PatientRepository(ProfilesDbContext context) : 
    BaseRepository<Patient>(context), IPatientRepository
{
    public async Task<(IReadOnlyList<Patient> Items, int TotalCount)> GetPagedAsync(
        string? firstName,
        string? lastName,
        DateOnly? birthDate,
        string? sortBy,
        bool isDescending,
        int pageNumber,
        int pageSize,
        CancellationToken ct)
    {
        var query = GetQuery(trackChanges: false);

        if (!string.IsNullOrWhiteSpace(firstName))
            query = query.Where(p => EF.Functions.ILike(p.FirstName, $"%{firstName}%"));

        if (!string.IsNullOrWhiteSpace(lastName))
            query = query.Where(p => EF.Functions.ILike(p.LastName, $"%{lastName}%"));
        
        if (birthDate.HasValue)
            query = query.Where(p => p.BirthDate == birthDate.Value);

        var totalCount = await query.CountAsync(ct);

        query = sortBy?.ToLower() switch
        {
            "firstname" => isDescending ? query.OrderByDescending(p => p.FirstName) : query.OrderBy(p => p.FirstName),
            "lastname" => isDescending ? query.OrderByDescending(p => p.LastName) : query.OrderBy(p => p.LastName),
            "birthdate" => isDescending ? query.OrderByDescending(p => p.BirthDate) : query.OrderBy(p => p.BirthDate),
            _ => query.OrderBy(p => p.Id)
        };

        var items = await query
            .ApplyPagination(pageNumber, pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
