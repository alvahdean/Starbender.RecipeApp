using Microsoft.Extensions.DependencyInjection;
using Starbender.RecipeApp.Core;
using Starbender.RecipeApp.Core.Extensions;
using Starbender.RecipeApp.Services.Contracts;

namespace Starbender.RecipeApp.Blazor;

public class RecipeBlazorModule : ModuleBase
{
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // Add dependent modules here...
        services.AddModule<RecipeServiceContractsModule>();

        // Add service registrations here...

        return services;
    }
}
