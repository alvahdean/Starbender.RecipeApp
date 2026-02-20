using Starbender.BlobStorage.Contracts;

namespace Starbender.RecipeApp.Services.Contracts.Dtos;

public class FullRecipeDto : RecipeDto
{
    /// <summary>
    /// The content for the image blob
    /// </summary>
    public BlobContentDto Image { get; set; } = new BlobContentDto();
}
