namespace Starbender.RecipeApp.Services.Contracts.Dtos;

// TODO: Add default units for the ingredient?
public class IngredientDto
{
    /// <summary>
    /// The id for the recipe instruction
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the ingredient
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Nav property to all the recipes that contain this ingredient (via mapping table)
    /// </summary>
    public HashSet<RecipeIngredientDto> RecipeIngredients { get; set; } = new HashSet<RecipeIngredientDto>();
}
