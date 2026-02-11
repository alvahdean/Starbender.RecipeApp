using AutoMapper;
using Starbender.RecipeApp.Domain.Shared.Entities;
using Starbender.RecipeApp.Services.Contracts.Dtos;

namespace Starbender.RecipeApp.Services.Contracts;

public class AutoMapperService : Profile
{
    public AutoMapperService()
    {

        CreateMap<Recipe, RecipeDto>()
            .ReverseMap();

        CreateMap<Instruction, InstructionDto>()
            .ReverseMap();

        CreateMap<Unit, UnitDto>()
            .ReverseMap();

        CreateMap<Ingredient, IngredientDto>()
            .ReverseMap();

        CreateMap<RecipeIngredient, RecipeIngredientDto>()
            .ReverseMap();
    }
}
