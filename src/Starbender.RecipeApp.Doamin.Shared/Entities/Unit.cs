using Starbender.RecipeApp.Core;

namespace Starbender.RecipeApp.Domain.Shared.Entities;

public class Unit : IHasId
{
    /// <summary>
    /// The id for the unit
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the unit
    /// </summary>
    public string Name { get; set; } = null!;
}
