using Starbender.Core;

namespace Starbender.RecipeApp.Services.Contracts.Dtos;

public class RecipeDto : IDto<int>
{
    /// <summary>
    /// The id for the recipe
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The title of the recipe
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// The description of the recipe
    /// </summary>
    public string Description { get; set; } = null!;

    // TODO: change name of ImagePath to ImageBlobId
    /// <summary>
    /// The path of image for the recipe (relative to ImageStoreRoot)
    /// </summary>
    public string? ImagePath { get; set; }

    /// <summary>
    /// Id of the user who created the recipe
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Indicates if the recipe is publicly viewable
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// Instuctions for the recipe
    /// </summary>
    public HashSet<InstructionDto> Instructions { get; set; } = new HashSet<InstructionDto>();

    /// <summary>
    /// Ingredients for the recipe
    /// </summary>
    public HashSet<RecipeIngredientDto> RecipeIngredients { get; set; } = new HashSet<RecipeIngredientDto>();
}
