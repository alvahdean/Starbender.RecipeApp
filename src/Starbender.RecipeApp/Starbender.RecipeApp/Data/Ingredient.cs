using System.ComponentModel.DataAnnotations.Schema;

namespace Starbender.RecipeApp.Data;

// TODO: Add default units for the ingredient?
public class Ingredient
{
    /// <summary>
    /// The id for the recipe instruction
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// The name of the ingredient
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Nav property to all the recipes that contain this ingredient (via mapping table)
    /// </summary>
    public HashSet<RecipeIngredient> RecipeIngredients { get; set; } = new HashSet<RecipeIngredient>();
}
