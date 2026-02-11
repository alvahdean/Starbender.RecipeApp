using Microsoft.EntityFrameworkCore;
using Starbender.RecipeApp.Core;
using Starbender.RecipeApp.Domain.Shared;
using System.Linq.Expressions;

namespace Starbender.RecipeApp.Domain;

public class EfRepository<TContext, TEntity> : EfRepository<TContext, TEntity,int>, IRepository<TEntity>
    where TEntity : class, IHasId<int>
    where TContext: DbContext
{
    public EfRepository(TContext db) : base(db) { }
}

public class EfRepository<TContext, TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class, IHasId<TKey>
    where TContext : DbContext
{
    private readonly TContext _db;
    private readonly DbSet<TEntity> _set;

    public EfRepository(TContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _set = _db.Set<TEntity>();
    }

    public async Task<TEntity?> GetAsync(TKey id, CancellationToken ct = default)
        => await _set.FindAsync([id], ct);

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default)
        => await _set.AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<TEntity>> QueryAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default)
        => await _set.AsNoTracking().Where(predicate).ToListAsync(ct);

    public async Task<TEntity> CreateAsync(TEntity entity, CancellationToken ct = default)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        await _set.AddAsync(entity, ct);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        _set.Update(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task DeleteAsync(TKey id, CancellationToken ct = default)
    {
        var entity = await GetAsync(id, ct);
        if (entity is null) return; // or throw, depending on preference

        _set.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(TKey id, CancellationToken ct = default)
        => await GetAsync(id, ct) is not null;
}
