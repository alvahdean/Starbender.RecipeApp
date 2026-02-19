using Starbender.BlobStorage.Contracts;
using Starbender.RecipeApp.Services.Contracts.Dtos;

namespace Starbender.RecipeApp.Services.Contracts;

public interface IRecipeAppService : ICrudAppService<RecipeDto>
{
    Task<BlobContentDto> SetRecipeImageAsync(int recipeId, byte[] imageBytes, CancellationToken ct);
    Task<BlobContentDto?> GetRecipeImageAsync(int recipeId, CancellationToken ct);
}
