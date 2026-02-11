using Starbender.RecipeApp.Core;
using Starbender.RecipeApp.Domain;
using Starbender.RecipeApp.Domain.Shared;

namespace Starbender.RecipeApp.EntityFrameworkCore.Repositories;

public class RecipeAppRepository<TEntity> : RecipeAppRepository<TEntity, int>, IRepository<TEntity>
    where TEntity : class, IHasId<int>
{
    public RecipeAppRepository(ApplicationDbContext db) : base(db) { }
}

public class RecipeAppRepository<TEntity, TKey> : EfRepository<ApplicationDbContext, TEntity, TKey>
    where TEntity : class, IHasId<TKey>
{
    public RecipeAppRepository(ApplicationDbContext db) : base(db)
    {

    }
}
