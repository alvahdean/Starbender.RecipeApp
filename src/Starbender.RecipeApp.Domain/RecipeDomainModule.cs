using Microsoft.Extensions.DependencyInjection;
using Starbender.Core;
using Starbender.Core.Extensions;
using Starbender.RecipeApp.Domain.Shared;

namespace Starbender.RecipeApp.Domain;

public class RecipeDomainModule : ModuleBase
{
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // Add dependent modules here...
        services.AddModule<RecipeDomainSharedModule>();

        // Add service registrations here...

        return services;
    }
}
