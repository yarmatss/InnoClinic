using Microsoft.EntityFrameworkCore;
using Profiles.DAL.Data;
using Profiles.DAL.Entities;
using Profiles.DAL.Extensions;
using Profiles.DAL.Interfaces;
using Profiles.DAL.Builders;
using Profiles.Domain.Models;

namespace Profiles.DAL.Repositories;

public class SpecializationRepository(ProfilesDbContext context) : 
    BaseRepository<Specialization>(context), ISpecializationRepository
{
    public async Task<(IReadOnlyList<Specialization> Items, int TotalCount)> GetPagedAsync(
        SpecializationQueryParameters parameters,
        CancellationToken ct)
    {
        var builder = new SpecializationQueryBuilder(GetQuery(trackChanges: false))
            .FilterByName(parameters.Name);

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
