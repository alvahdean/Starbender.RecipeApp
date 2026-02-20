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

    /// <summary>
    /// The blob id for the recipe image
    /// </summary>
    public string? ImageBlobId { get; set; }

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
    public string? Instructions { get; set; }

    /// <summary>
    /// Ingredients for the recipe
    /// </summary>
    public ICollection<RecipeIngredientDto> RecipeIngredients { get; set; } = new List<RecipeIngredientDto>();

    public string IngredientText => string.Join("\n", RecipeIngredients.Select(t => t.ToString()));
}
