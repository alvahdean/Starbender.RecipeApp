using Starbender.BlobStorage.Contracts;
using Starbender.RecipeApp.Domain.Shared.Entities;
using Starbender.RecipeApp.Services.Contracts.Dtos;

namespace Starbender.RecipeApp.Services.Contracts;

public interface IRecipeAppService : ICrudAppService<RecipeDto>
{
    Task<BlobMetadataDto> SetRecipeImage(int recipeId, byte[] imageBytes, CancellationToken ct);
    Task<byte[]> GetRecipeImageAsync(int recipeId, CancellationToken ct = default);
}
