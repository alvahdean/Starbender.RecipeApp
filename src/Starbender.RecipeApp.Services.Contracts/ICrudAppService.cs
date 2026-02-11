using Starbender.RecipeApp.Core;
using System.Linq.Expressions;

namespace Starbender.RecipeApp.Services.Contracts;

public interface ICrudAppService<TDto> : ICrudAppService<TDto, int>
    where TDto : class, IHasId<int>
{
}

public interface ICrudAppService<TDto, TKey>
    where TDto : class, IHasId<TKey>
{
    Task<TDto?> GetAsync(TKey id, CancellationToken ct = default);

    Task<IReadOnlyList<TDto>> GetAllAsync(CancellationToken ct = default);

    Task<TDto> CreateAsync(TDto dto, CancellationToken ct = default);

    Task<TDto> UpdateAsync(TDto dto, CancellationToken ct = default);

    Task DeleteAsync(TKey id, CancellationToken ct = default);

    Task<bool> ExistsAsync(TKey id, CancellationToken ct = default);
}
