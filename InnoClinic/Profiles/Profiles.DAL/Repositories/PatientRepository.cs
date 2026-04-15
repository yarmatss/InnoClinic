using Profiles.DAL.Data;
using Profiles.DAL.Entities;
using Profiles.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Profiles.DAL.Extensions;
using Profiles.Domain.Models;
using Profiles.DAL.Builders;

namespace Profiles.DAL.Repositories;

public class PatientRepository(ProfilesDbContext context) : 
    BaseRepository<Patient>(context), IPatientRepository
{
    public async Task<(IReadOnlyList<Patient> Items, int TotalCount)> GetPagedAsync(
        PatientQueryParameters parameters,
        CancellationToken ct)
    {
        var builder = new PatientQueryBuilder(GetQuery(trackChanges: false))
            .FilterByFirstName(parameters.FirstName)
            .FilterByLastName(parameters.LastName)
            .FilterByBirthDate(parameters.BirthDate);

        var query = builder.Build();
        var totalCount = await query.CountAsync(ct);

        var items = await builder
            .SortBy(parameters.SortBy, parameters.IsDescending)
            .Build()
            .ApplyPagination(parameters.PageNumber!.Value, parameters.PageSize!.Value)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
