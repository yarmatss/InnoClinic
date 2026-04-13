using Microsoft.EntityFrameworkCore;
using Profiles.DAL.Data;
using Profiles.DAL.Entities;
using Profiles.DAL.Extensions;
using Profiles.DAL.Interfaces;

namespace Profiles.DAL.Repositories;

public class SpecializationRepository(ProfilesDbContext context) : 
    BaseRepository<Specialization>(context), ISpecializationRepository
{
    public async Task<(IReadOnlyList<Specialization> Items, int TotalCount)> GetPagedAsync(
        string? name,
        string? sortBy,
        bool isDescending,
        int pageNumber,
        int pageSize,
        CancellationToken ct)
    {
        var query = GetQuery(trackChanges: false);

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(s => EF.Functions.ILike(s.Name, $"%{name}%"));

        var totalCount = await query.CountAsync(ct);

        query = sortBy?.ToLower() switch
        {
            "name" => isDescending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
            _ => query.OrderBy(s => s.Name)
        };

        var items = await query.ApplyPagination(
            pageNumber,
            pageSize).ToListAsync(ct);

        return (items, totalCount);
    }
}
