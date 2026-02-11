using AutoMapper;
using Microsoft.Extensions.Logging;
using Starbender.RecipeApp.Domain.Shared;
using Starbender.RecipeApp.Domain.Shared.Entities;
using Starbender.RecipeApp.Services.Contracts;
using Starbender.RecipeApp.Services.Contracts.Dtos;

namespace Starbender.RecipeApp.Services;

public class RecipeAppService : CrudAppService<Recipe, RecipeDto>, IRecipeAppService
{
    public RecipeAppService(
    IMapper mapper,
    ILogger<RecipeAppService> logger,
    IRepository<Recipe> repo) : base(mapper, logger, repo)
    {
    }
}
