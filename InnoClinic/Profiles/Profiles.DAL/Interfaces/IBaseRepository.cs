using Profiles.DAL.Entities;
using System.Linq.Expressions;

namespace Profiles.DAL.Interfaces;

public interface IBaseRepository<T> where T : BaseEntity
{
    Task<IReadOnlyList<T>> GetAllAsync(
        CancellationToken ct, 
        bool trackChanges = false);
    Task<IReadOnlyList<T>> GetByConditionAsync(
        Expression<Func<T, bool>> expression, 
        CancellationToken ct, 
        bool trackChanges = false);
    Task<T?> GetByIdAsync(
        Guid id, 
        CancellationToken ct,
        bool trackChanges = false);
    void MarkAdd(T entity);
    void MarkUpdate(T entity);
    void MarkDelete(T entity);
    Task SaveChangesAsync(CancellationToken ct);
}
