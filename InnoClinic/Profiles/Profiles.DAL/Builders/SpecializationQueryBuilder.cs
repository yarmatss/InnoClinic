using Microsoft.EntityFrameworkCore;
using Profiles.DAL.Entities;

namespace Profiles.DAL.Builders;

internal class SpecializationQueryBuilder(IQueryable<Specialization> query)
{
    public SpecializationQueryBuilder FilterByName(string? name)
    {
        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(s => EF.Functions.ILike(s.Name, $"%{name}%"));
        return this;
    }

    public SpecializationQueryBuilder SortBy(string? sortBy, bool isDescending)
    {
        query = sortBy?.ToLower() switch
        {
            "name" => isDescending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
            _ => query.OrderBy(s => s.Name)
        };
        return this;
    }

    public IQueryable<Specialization> Build() => query;
}
