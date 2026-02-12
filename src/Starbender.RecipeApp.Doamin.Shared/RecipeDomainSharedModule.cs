using Microsoft.Extensions.DependencyInjection;
using Starbender.Core;

namespace Starbender.RecipeApp.Domain.Shared;

public class RecipeDomainSharedModule : ModuleBase
{
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // Add service registrations here...

        return services;
    }
}
