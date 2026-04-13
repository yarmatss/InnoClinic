using Microsoft.EntityFrameworkCore;
using Profiles.DAL.Data;
using Profiles.DAL.Entities;
using Profiles.DAL.Interfaces;
using System.Linq.Expressions;

namespace Profiles.DAL.Repositories;

public abstract class BaseRepository<T>(ProfilesDbContext context) : IBaseRepository<T> where T : BaseEntity
{
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    protected IQueryable<T> GetQuery(bool trackChanges, bool ignoreQueryFilters = false)
    {
        var query = trackChanges ? _dbSet : _dbSet.AsNoTracking();

        if (ignoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        return query;
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(
        CancellationToken ct,
        bool trackChanges = false)
    {
        return await GetQuery(trackChanges).ToListAsync(ct);
    }

    public virtual async Task<IReadOnlyList<T>> GetByConditionAsync(
        Expression<Func<T, bool>> expression, 
        CancellationToken ct,
        bool trackChanges = false)
    {
        return await GetQuery(trackChanges).Where(expression).ToListAsync(ct);
    }

    public virtual async Task<T?> GetByIdAsync(
        Guid id, 
        CancellationToken ct,
        bool trackChanges = false)
    {
        return await GetQuery(trackChanges).FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public virtual void MarkAdd(T entity)
    {
        _dbSet.Add(entity);
    }

    public virtual void MarkUpdate(T entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void MarkDelete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual async Task SaveChangesAsync(CancellationToken ct)
    {
        await context.SaveChangesAsync(ct);
    }
}
