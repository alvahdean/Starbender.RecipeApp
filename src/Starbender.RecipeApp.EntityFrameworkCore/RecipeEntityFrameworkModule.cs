using Microsoft.Extensions.DependencyInjection;
using Starbender.RecipeApp.Core;
using Starbender.RecipeApp.Core.Extensions;
using Starbender.RecipeApp.Domain;

namespace Starbender.RecipeApp.EntityFrameworkCore;

public class RecipeEntityFrameworkModule : ModuleBase
{
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // Add service registrations here...
        services.AddModule<RecipeDomainModule>();
        return services;
    }
}
