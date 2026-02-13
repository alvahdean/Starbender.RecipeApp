using Starbender.Core;

namespace Starbender.RecipeApp.Domain.Shared.Entities;

// TODO: Add default units for the ingredient?
public class Ingredient : IEntity<int>
{
    /// <summary>
    /// The id for the recipe instruction
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the ingredient
    /// </summary>
    public string Name { get; set; } = null!;
}
