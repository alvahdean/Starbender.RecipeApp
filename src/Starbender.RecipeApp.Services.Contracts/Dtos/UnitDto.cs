using Starbender.RecipeApp.Core;

namespace Starbender.RecipeApp.Services.Contracts.Dtos;

public class UnitDto : IHasId
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
