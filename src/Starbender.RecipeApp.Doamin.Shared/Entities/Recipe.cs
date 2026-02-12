using Starbender.BlobStorage.Entities;
using Starbender.Core;

namespace Starbender.RecipeApp.Domain.Shared.Entities;

public class Recipe : IEntity<int>
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
    public int? ImageMetadataId { get; set; }

    /// <summary>
    /// Id of the user who created the recipe
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Indicates if the recipe is publicly viewable
    /// </summary>
    public bool IsPublic { get; set; }

    public BlobMetadata? ImageMetadata { get; set; }

    /// <summary>
    /// Instuctions for the recipe
    /// </summary>
    public HashSet<Instruction> Instructions { get; set; } = new HashSet<Instruction>();

    /// <summary>
    /// Ingredients for the recipe
    /// </summary>
    public HashSet<RecipeIngredient> RecipeIngredients { get; set; } = new HashSet<RecipeIngredient>();
}
