using AutoMapper;
using Microsoft.Extensions.Logging;
using Starbender.Core;
using Starbender.RecipeApp.Domain.Shared.Entities;
using Starbender.RecipeApp.Services.Contracts;
using Starbender.RecipeApp.Services.Contracts.Dtos;

namespace Starbender.RecipeApp.Services;

public class IngredientAppService : CrudAppService<Ingredient, IngredientDto>, IIngredientAppService
{
    public IngredientAppService(
    IMapper mapper,
    ILogger<IngredientAppService> logger,
    IRepository<Ingredient> repo) : base(mapper, logger, repo)
    {
    }
}
