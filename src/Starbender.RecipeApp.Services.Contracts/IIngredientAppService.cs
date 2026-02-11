using Starbender.RecipeApp.Domain.Shared.Entities;
using Starbender.RecipeApp.Services.Contracts.Dtos;

namespace Starbender.RecipeApp.Services.Contracts;

public interface IIngredientAppService : ICrudAppService<Ingredient, IngredientDto>
{
}
