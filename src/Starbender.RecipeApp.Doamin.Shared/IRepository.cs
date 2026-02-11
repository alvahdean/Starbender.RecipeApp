using System.Linq.Expressions;

namespace Starbender.RecipeApp.Domain.Shared;

public interface IRepository<TEntity> : IRepository<TEntity, int>
    where TEntity : class
{
}

public interface IRepository<TEntity, TKey>
    where TEntity : class
    where TKey : notnull
{
    Task<TEntity?> GetAsync(TKey id, CancellationToken ct = default);

    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default);

    Task<IReadOnlyList<TEntity>> QueryAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default);

    Task<TEntity> CreateAsync(TEntity entity, CancellationToken ct = default);

    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken ct = default);

    Task DeleteAsync(TKey id, CancellationToken ct = default);

    Task<bool> ExistsAsync(TKey id, CancellationToken ct = default);
}
