using Microsoft.Extensions.DependencyInjection;
using Starbender.RecipeApp.Core;
using Starbender.RecipeApp.Core.Extensions;
using Starbender.RecipeApp.Services.Contracts;

namespace Starbender.RecipeApp.Blazor;

public class RecipeBlazorModule : ModuleBase
{
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // Add service registrations here...
        services.AddModule<RecipeServiceContractsModule>();
        return services;
    }
}
