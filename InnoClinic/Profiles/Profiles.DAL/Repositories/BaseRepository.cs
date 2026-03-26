using Microsoft.EntityFrameworkCore;
using Profiles.DAL.Data;
using Profiles.DAL.Interfaces;
using System.Linq.Expressions;

namespace Profiles.DAL.Repositories;

public abstract class BaseRepository<T>(ProfilesDbContext context) : IBaseRepository<T> where T : class
{
    protected readonly ProfilesDbContext _context = context;
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    public virtual IQueryable<T> GetAll() => _dbSet.AsNoTracking();

    public virtual IQueryable<T> GetByCondition(Expression<Func<T, bool>> expression) =>
        _dbSet
        .Where(expression)
        .AsNoTracking();

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await _dbSet.FindAsync([id], cancellationToken: ct);

    public virtual void MarkAdd(T entity) =>
        _dbSet.Add(entity);

    public virtual void Update(T entity) =>
        _dbSet.Update(entity);

    public virtual void MarkDelete(T entity) =>
        _dbSet.Remove(entity);

    public virtual async Task SaveChangesAsync(CancellationToken ct) =>
        await _context.SaveChangesAsync(ct);
}
