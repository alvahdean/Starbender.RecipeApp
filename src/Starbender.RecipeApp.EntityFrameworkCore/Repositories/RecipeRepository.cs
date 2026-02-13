using Microsoft.EntityFrameworkCore;
using Starbender.Core;
using Starbender.RecipeApp.Domain.Shared.Entities;
using System.Linq.Expressions;

namespace Starbender.RecipeApp.EntityFrameworkCore.Repositories;

public class RecipeRepository : IRepository<Recipe>
{
    private readonly ApplicationDbContext _db;
    private readonly DbSet<Recipe> _set;

    public RecipeRepository(ApplicationDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _set = _db.Set<Recipe>();
    }

    public async Task<Recipe?> GetAsync(int id, CancellationToken ct = default)
        => await QueryWithIncludes().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Recipe>> GetAllAsync(CancellationToken ct = default)
        => await QueryWithIncludes().ToListAsync(ct);

    public IQueryable<Recipe> Query(Expression<Func<Recipe, bool>> predicate)
        => QueryWithIncludes().Where(predicate);

    public async Task<Recipe> CreateAsync(Recipe entity, CancellationToken ct = default)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        await _set.AddAsync(entity, ct);
        await _db.SaveChangesAsync(ct);
        return await GetAsync(entity.Id, ct) ?? entity;
    }

    public async Task<Recipe> UpdateAsync(Recipe entity, CancellationToken ct = default)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        var existing = await _set
            .Include(x => x.RecipeIngredients)
            .FirstOrDefaultAsync(x => x.Id == entity.Id, ct);

        if (existing is null)
        {
            _set.Update(entity);
            await _db.SaveChangesAsync(ct);
            return await GetAsync(entity.Id, ct) ?? entity;
        }

        existing.Title = entity.Title;
        existing.Description = entity.Description;
        existing.ImageBlobId = entity.ImageBlobId;
        existing.UserId = entity.UserId;
        existing.IsPublic = entity.IsPublic;
        existing.Instructions = entity.Instructions;

        var incomingIngredients = entity.RecipeIngredients
            .Where(x => x.IngredientId > 0)
            .GroupBy(x => x.IngredientId)
            .Select(x => x.Last())
            .ToDictionary(x => x.IngredientId);

        foreach (var current in existing.RecipeIngredients.ToList())
        {
            if (!incomingIngredients.TryGetValue(current.IngredientId, out var incoming))
            {
                existing.RecipeIngredients.Remove(current);
                continue;
            }

            current.Quantity = incoming.Quantity;
            current.UnitId = incoming.UnitId;
        }

        var existingIngredientIds = existing.RecipeIngredients
            .Select(x => x.IngredientId)
            .ToHashSet();

        foreach (var incoming in incomingIngredients.Values)
        {
            if (existingIngredientIds.Contains(incoming.IngredientId))
            {
                continue;
            }

            existing.RecipeIngredients.Add(new RecipeIngredient
            {
                RecipeId = existing.Id,
                IngredientId = incoming.IngredientId,
                Quantity = incoming.Quantity,
                UnitId = incoming.UnitId
            });
        }

        await _db.SaveChangesAsync(ct);
        return await GetAsync(existing.Id, ct) ?? existing;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _set.FindAsync([id], ct);
        if (entity is null)
        {
            return;
        }

        _set.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => await _set.AsNoTracking().AnyAsync(x => x.Id == id, ct);

    private IQueryable<Recipe> QueryWithIncludes()
        => _set.AsNoTracking()
            .Include(x => x.RecipeIngredients)
            .ThenInclude(x => x.Ingredient)
            .Include(x => x.RecipeIngredients)
            .ThenInclude(x => x.Unit);
}
