using Starbender.Core;

namespace Starbender.RecipeApp.Services.Contracts.Dtos;

public class InstructionDto : IDto<int>
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
    public RecipeDto Recipe { get; set; } = null!;
}
