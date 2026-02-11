namespace Starbender.RecipeApp.Domain.Shared.Models;

public class Instruction
{
    /// <summary>
    /// The id for the recipe instruction
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The id for the recipe the instruction
    /// </summary>
    public int RecipeId { get; set; }

    /// <summary>
    /// The text of the recipe instruction
    /// </summary>
    public string Text { get; set; } = null!;

    /// <summary>
    /// The order of the instruction in the recipe
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Nav property to the Recipe
    /// </summary>
    public Recipe Recipe { get; set; } = null!;
}
