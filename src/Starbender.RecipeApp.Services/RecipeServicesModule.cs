using Microsoft.Extensions.DependencyInjection;
using Starbender.RecipeApp.Core;
using Starbender.RecipeApp.Core.Extensions;
using Starbender.RecipeApp.Domain;
using Starbender.RecipeApp.EntityFrameworkCore;
using Starbender.RecipeApp.Services.Contracts;

namespace Starbender.RecipeApp.Services;

public class RecipeServicesModule : ModuleBase
{
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // Add dependent modules here...
        services.AddModule<RecipeServiceContractsModule>();
        services.AddModule<RecipeDomainModule>();
        services.AddModule<RecipeEntityFrameworkModule>();

        // Register local services here...
        services.AddTransient<IRecipeAppService, RecipeAppService>();
        services.AddTransient<IInstructionAppService, InstructionAppService>();
        services.AddTransient<IIngredientAppService, IngredientAppService>();
        services.AddTransient<IUnitAppService, UnitAppService>();

        return services;
    }
}
