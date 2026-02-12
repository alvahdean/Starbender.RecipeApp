using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Starbender.BlobStorage.Contracts;
using Starbender.Core;
using Starbender.RecipeApp.Domain.Shared.Entities;
using Starbender.RecipeApp.Services.Contracts;
using Starbender.RecipeApp.Services.Contracts.Dtos;
using Starbender.RecipeApp.Services.Contracts.Options;

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

    public async Task<BlobMetadataDto> SetRecipeImage(int recipeId, byte[] imageBytes, CancellationToken ct=default)
    {
        var container = _containerFactory.GetContainer(_options.ImageStoreType, _options.ImageContainerId)
            ?? throw new Exception($"No Recipe Image container found. ({_options.ImageStoreType}.{_options.ImageContainerId})");

        var recipe = await GetAsync(recipeId, ct)
            ?? throw new Exception($"No Recipe with ID={recipeId} found.");

        // TODO: change name of ImagePath to ImageBlobId
        var blobId = recipe.ImagePath;

        BlobMetadataDto? blobInfo;
        if (blobId == null)
        {
            blobInfo = await container.CreateContentAsync(imageBytes, ct);
            recipe.ImagePath = blobInfo.BlobId;
            await UpdateAsync(recipe);
        }
        else
        {
            blobInfo = await container.UpdateContentAsync(blobId, imageBytes, ct);
            if (blobInfo.BlobId != blobId)
            {
                recipe.ImagePath = blobInfo.BlobId;
                await UpdateAsync(recipe);
            }
        }

        return blobInfo;
    }

    public async Task<byte[]> GetRecipeImageAsync(int recipeId, CancellationToken ct=default)
    {
        var container = GetImageContainer() 
            ?? throw new Exception($"No Recipe Image Blob Container found.");

        var recipe = await GetAsync(recipeId, ct)
            ?? throw new Exception($"No Recipe with ID={recipeId} found.");

        
        var blobId = recipe.ImagePath;
        var result = await container.GetContentAsync(blobId, ct);

        return result;
    }

    private IBlobContainer? GetImageContainer() =>
        _containerFactory.GetContainer(_options.ImageStoreType, _options.ImageContainerId);

}
