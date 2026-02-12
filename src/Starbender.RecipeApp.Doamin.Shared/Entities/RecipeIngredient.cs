using Starbender.Core;

namespace Starbender.RecipeApp.Domain.Shared.Entities;

public class RecipeIngredient : IEntity
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
    public Recipe Recipe { get; set; } = null!;

    /// <summary>
    /// Nav property to the ingredient
    /// </summary>
    public Ingredient Ingredient { get; set; } = null!;

    /// <summary>
    /// Nav property to the unit
    /// </summary>
    public Unit? Unit { get; set; }
}
