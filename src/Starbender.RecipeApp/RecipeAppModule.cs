using Microsoft.Extensions.DependencyInjection;
using Starbender.RecipeApp.Core;
using Starbender.RecipeApp.Core.Extensions;
using Starbender.RecipeApp.Services;

namespace Starbender.RecipeApp;

public class RecipeAppModule : ModuleBase
{
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // Add service registrations here...
        services.AddModule<RecipeServicesModule>();
        return services;
    }
}
