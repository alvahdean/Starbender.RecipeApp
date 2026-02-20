using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Starbender.BlobStorage.Contracts;
using Starbender.Core;
using Starbender.RecipeApp.Domain.Shared.Entities;
using Starbender.RecipeApp.Services.Contracts;
using Starbender.RecipeApp.Services.Contracts.Dtos;

namespace Starbender.RecipeApp.Services;

public class RecipeAppService : CrudAppService<Recipe, RecipeDto>, IRecipeAppService
{
    public const string ContentType = "image/png";

    //private readonly IBlobContainerFactory _containerFactory;
    private readonly RecipeAppOptions _options;
    private readonly IBlobContainer _container;

    public RecipeAppService(
        IMapper mapper,
        IOptions<RecipeAppOptions> options,
        IBlobContainerFactory containerFactory,
        ILogger<RecipeAppService> logger,
        IRepository<Recipe> repo) : base(mapper, logger, repo)
    {
        _options = options.Value;
        _container = containerFactory.GetContainer(_options.ImageContainerId)
            ?? throw new Exception($"Blob container '{_options.ImageStoreType}.{_options.ImageContainerId}' not found");
    }

    public virtual async Task<FullRecipeDto?> GetFullAsync(int recipeId, CancellationToken ct = default)
    {

        var thinDto = await base.GetAsync(recipeId, ct);

        var result = Mapper.Map<FullRecipeDto>(thinDto);

        if (result!=null && !string.IsNullOrWhiteSpace(result.ImageBlobId))
        {
            result.Image = await GetRecipeImageAsync(recipeId, ct) ?? new();
        }

        return result;
    }

    public virtual async Task<FullRecipeDto> UpdateFullAsync(FullRecipeDto updated, CancellationToken ct = default)
    {
        var recipeId = updated.Id;

        var thinDto = await base.UpdateAsync(updated, ct);

        var result = Mapper.Map<FullRecipeDto>(thinDto);

        var byteData = updated.Image.Content;

        if (byteData.Length > 0)
        {
            result.Image = await SetRecipeImageAsync(recipeId, byteData, ct);
        }

        return result;
    }

    public async Task<BlobContentDto> SetRecipeImageAsync(int recipeId, byte[] imageBytes, CancellationToken ct = default)
    {
        var recipe = await GetAsync(recipeId, ct);

        var blobId = recipe?.ImageBlobId;

        BlobContentDto? blobInfo;
        if (blobId == null)
        {
            blobInfo = await _container.CreateContentAsync(imageBytes, ContentType, ct);

            if (recipe != null && recipe.ImageBlobId !=blobInfo.BlobId)
            {
                recipe.ImageBlobId = blobInfo.BlobId;
                await UpdateAsync(recipe);
            }
        }
        else
        {
            blobInfo = await _container.UpdateContentAsync(blobId,imageBytes,ContentType, ct);

            if (blobInfo.BlobId != blobId && recipe != null)
            {
                recipe.ImageBlobId = blobInfo.BlobId;
                await UpdateAsync(recipe);
            }
        }

        return blobInfo;
    }

    public async Task<BlobContentDto?> GetRecipeImageAsync(int recipeId, CancellationToken ct = default)
    {
        var recipe = await GetAsync(recipeId, ct);

        if (recipe == null)
        {
            return null;
        }

        var blobId = recipe.ImageBlobId;

        BlobContentDto? result = null;

        if (!string.IsNullOrWhiteSpace(blobId))
        {
            try
            {
                result = await _container.GetContentAsync(blobId, ct);
            } catch
            {
                Logger.LogWarning("ImageBlob not found in container, returning null");
            }
        }

        return result;
    }
}
