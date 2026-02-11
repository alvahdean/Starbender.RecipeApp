using Microsoft.Extensions.DependencyInjection;

namespace Starbender.RecipeApp.Core;

public class RecipeCoreModule : ModuleBase
{
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // Add service registrations here...

        return services;
    }
}
