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
    private readonly IBlobContainerFactory _containerFactory;
    private readonly RecipeAppOptions _options;

    public RecipeAppService(
        IMapper mapper,
        IOptions<RecipeAppOptions> options,
        IBlobContainerFactory containerFactory,
        ILogger<RecipeAppService> logger,
        IRepository<Recipe> repo) : base(mapper, logger, repo)
    {
        _containerFactory = containerFactory;
        _options = options.Value;
    }

    public virtual async Task<FullRecipeDto> GetFullAsync(int recipeId, CancellationToken ct = default)
    {

        var thinDto = await base.GetAsync(recipeId, ct);

        var result = Mapper.Map<FullRecipeDto>(thinDto);

        if (string.IsNullOrWhiteSpace(result.ImageBlobId))
        {
            result.Image = await GetRecipeImageAsync(recipeId, ct);
        }

        return result;
    }

    public virtual async Task<FullRecipeDto> UpdateFullAsync(FullRecipeDto updated, CancellationToken ct = default)
    {
        var recipeId = updated.Id;

        var thinDto = await base.UpdateAsync(updated, ct);

        var result = Mapper.Map<FullRecipeDto>(thinDto);

        var byteData = updated.Image?.Content ?? Array.Empty<byte>();

        if (byteData.Length > 0)
        {
            result.Image = await SetRecipeImageAsync(recipeId, byteData, ct);
        }

        return result;
    }

    public async Task<BlobContentDto> SetRecipeImageAsync(int recipeId, byte[] imageBytes, CancellationToken ct = default)
    {
        var container = _containerFactory.GetContainer(_options.ImageContainerId)
            ?? throw new Exception($"No Recipe Image container found. ({_options.ImageStoreType}.{_options.ImageContainerId})");

        var recipe = await GetAsync(recipeId, ct);

        var blobId = recipe?.ImageBlobId;

        BlobContentDto? blobInfo;
        if (blobId == null)
        {
            blobInfo = await container.CreateContentAsync(new BlobContentDto()
            {
                Content = imageBytes,
                ContentType = "image/png"
            }, ct);

            if (recipe != null)
            {
                recipe.ImageBlobId = blobInfo.BlobId;
                await UpdateAsync(recipe);
            }
        }
        else
        {
            blobInfo = await container.UpdateContentAsync(new BlobContentDto()
            {
                Content = imageBytes,
                BlobId = blobId
            }, ct);

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
        var container = GetImageContainer();

        if (container == null)
        {
            return null;
        }

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
                result = await container.GetContentAsync(blobId, ct);
            } catch
            {
                Logger.LogWarning("ImageBlob not found in container, returning null");
            }
        }

        return result;
    }

    private IBlobContainer? GetImageContainer() =>
        _containerFactory.GetContainer(_options.ImageContainerId);
}
