using Microsoft.Extensions.DependencyInjection;
using Starbender.RecipeApp.Core;

namespace Starbender.RecipeApp.Services;

public class RecipeServicesModule : ModuleBase
{
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // Add service registrations here...

        return services;
    }
}
