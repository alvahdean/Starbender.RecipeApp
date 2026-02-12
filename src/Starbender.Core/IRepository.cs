using System.Linq.Expressions;

namespace Starbender.Core;

public interface IRepository<TEntity> : IRepository<TEntity, int>
    where TEntity : class, IHasId<int>
{
}

public interface IRepository<TEntity, TKey>
    where TEntity : class, IHasId<TKey>
{
    Task<TEntity?> GetAsync(TKey id, CancellationToken ct);

    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct);

    IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate);

    Task<TEntity> CreateAsync(TEntity entity, CancellationToken ct);

    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken ct);

    Task DeleteAsync(TKey id, CancellationToken ct);

    Task<bool> ExistsAsync(TKey id, CancellationToken ct);
}
