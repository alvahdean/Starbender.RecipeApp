using AutoMapper;
using Microsoft.Extensions.Logging;
using Starbender.Core;
using Starbender.RecipeApp.Domain.Shared.Entities;
using Starbender.RecipeApp.Services.Contracts;
using Starbender.RecipeApp.Services.Contracts.Dtos;

namespace Starbender.RecipeApp.Services;

public class UnitAppService : CrudAppService<Unit, UnitDto>, IUnitAppService
{
    public UnitAppService(
    IMapper mapper,
    ILogger<UnitAppService> logger,
    IRepository<Unit> repo) : base(mapper, logger, repo)
    {
    }
}
