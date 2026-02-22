using AutoMapper;
using Microsoft.Extensions.Logging;
using Starbender.Core;
using Starbender.RecipeApp.Services.Contracts;

namespace Starbender.RecipeApp.Services;

public class CrudAppService<TEntity, TDto> : CrudAppService<TEntity, TDto, int>, ICrudAppService<TDto>
    where TEntity : class, IHasId<int>
    where TDto : class, IHasId<int>
{
    public CrudAppService(
        IMapper mapper,
        ILogger logger,
        IRepository<TEntity, int> repo) : base(mapper, logger, repo)
    {
    }
}

public class CrudAppService<TEntity, TDto, TKey> : ICrudAppService<TDto, TKey>
    where TEntity : class, IHasId<TKey>
    where TDto : class, IHasId<TKey>
    where TKey: notnull
{
    protected IMapper Mapper { get; init; }
    protected ILogger Logger { get; init; }

    protected IRepository<TEntity, TKey> Repo { get; init; }

    public CrudAppService(
        IMapper mapper,
        ILogger logger,
        IRepository<TEntity, TKey> repo)
    {
        Mapper = mapper;
        Logger = logger;
        Repo = repo;
    }

    public virtual async Task<TDto?> GetAsync(TKey id, CancellationToken ct = default)
    {
        var entity = await Repo.GetAsync(id, ct);
        var dto = Mapper.Map<TDto>(entity);

        return dto;
    }

    public virtual async Task<IReadOnlyList<TDto>> GetAllAsync(CancellationToken ct = default)
    {
        var entities = await Repo.GetAllAsync(ct);
        var dtos = Mapper.Map<IEnumerable<TDto>>(entities);

        return dtos.ToList();
    }

    public virtual async Task<TDto> CreateAsync(TDto dto, CancellationToken ct = default)
    {
        var entity = Mapper.Map<TEntity>(dto);
        entity = await Repo.CreateAsync(entity, ct);
        var result = Mapper.Map<TDto>(entity);

        return result;
    }

    public virtual async Task<TDto> UpdateAsync(TDto dto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (dto.Id.Equals(default(TKey)))
        {
            return await CreateAsync(dto, ct);
        }

        var entity = await Repo.GetAsync(dto.Id, ct);

        if (entity == null)
        {
            return await CreateAsync(dto, ct);
        }

        Mapper.Map(dto, entity);

        var updated = await Repo.UpdateAsync(entity, ct);

        var result = Mapper.Map<TDto>(updated);

        return result;
    }

    public virtual async Task DeleteAsync(TKey id, CancellationToken ct = default)
    {
        await Repo.DeleteAsync(id, ct);
    }

    public virtual async Task<bool> ExistsAsync(TKey id, CancellationToken ct = default)
    {
        var result = await Repo.ExistsAsync(id, ct);

        return result;
    }
}
