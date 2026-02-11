namespace Starbender.RecipeApp.Services.Contracts.Dtos;

public class RecipeIngredientDto 
{
    /// <summary>
    /// Id of the recipe
    /// </summary>
    public int RecipeId { get; set; }

    /// <summary>
    /// Id for the ingredient
    /// </summary>
    public int IngredientId { get; set; }

    /// <summary>
    /// THe quantity of the specified unit for the ingredient
    /// </summary>
    public double Quantity { get; set; }

    /// <summary>
    /// Id for the type of unit for the ingredient
    /// </summary>
    public int? UnitId { get; set; }

    /// <summary>
    /// Nav property to the recipe
    /// </summary>
    public RecipeDto Recipe { get; set; } = null!;

    /// <summary>
    /// Nav property to the ingredient
    /// </summary>
    public IngredientDto Ingredient { get; set; } = null!;

    /// <summary>
    /// Nav property to the unit
    /// </summary>
    public UnitDto? Unit { get; set; }
}
