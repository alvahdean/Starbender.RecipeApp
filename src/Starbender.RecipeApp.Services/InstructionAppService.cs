using AutoMapper;
using Microsoft.Extensions.Logging;
using Starbender.Core;
using Starbender.RecipeApp.Domain.Shared.Entities;
using Starbender.RecipeApp.Services.Contracts;
using Starbender.RecipeApp.Services.Contracts.Dtos;

namespace Starbender.RecipeApp.Services;

public class InstructionAppService : CrudAppService<Instruction, InstructionDto>, IInstructionAppService
{
    public InstructionAppService(
    IMapper mapper,
    ILogger<InstructionAppService> logger,
    IRepository<Instruction> repo) : base(mapper, logger, repo)
    {
    }
}
