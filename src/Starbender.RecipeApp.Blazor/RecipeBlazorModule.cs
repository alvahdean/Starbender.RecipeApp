using Microsoft.Extensions.DependencyInjection;
using Starbender.Core;
using Starbender.Core.Extensions;
using Starbender.RecipeApp.Services.Contracts;

namespace Starbender.RecipeApp.Blazor;

public class RecipeBlazorModule : ModuleBase
{
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // Add dependent modules here...
        services.AddModule<RecipeServiceContractsModule>();

        // Add service registrations here...
        services.AddSingleton<RecipeSeeder>();

        return services;
    }
}
