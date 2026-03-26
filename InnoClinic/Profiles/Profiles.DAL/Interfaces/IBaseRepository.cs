using System.Linq.Expressions;

namespace Profiles.DAL.Interfaces;

public interface IBaseRepository<T> where T : class
{
    IQueryable<T> GetAll();
    IQueryable<T> GetByCondition(Expression<Func<T, bool>> expression);
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct);
    void MarkAdd(T entity);
    void Update(T entity);
    void MarkDelete(T entity);
    Task SaveChangesAsync(CancellationToken ct);
}