using AutoMapper;
using Starbender.RecipeApp.Domain.Shared.Entities;
using Starbender.RecipeApp.Services.Contracts.Dtos;

namespace Starbender.RecipeApp.Services.Contracts;

public class RecipeAppServiceContractsMapper : Profile
{
    public RecipeAppServiceContractsMapper()
    {

        CreateMap<Recipe, RecipeDto>()
            .ReverseMap()
            .ForMember(t=>t.Id,o=>o.Ignore());

        CreateMap<Unit, UnitDto>()
            .ReverseMap()
            .ForMember(t => t.Id, o => o.Ignore());

        CreateMap<Ingredient, IngredientDto>()
            .ReverseMap()
            .ForMember(t => t.Id, o => o.Ignore());

        CreateMap<RecipeIngredient, RecipeIngredientDto>()
            .ReverseMap()
            .ForMember(t => t.Ingredient, o => o.Ignore())
            .ForMember(t => t.Unit, o => o.Ignore());
    }
}
