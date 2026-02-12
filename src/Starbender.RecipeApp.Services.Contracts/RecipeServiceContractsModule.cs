using Microsoft.Extensions.DependencyInjection;
using Starbender.Core;
using Starbender.Core.Extensions;
using Starbender.RecipeApp.Domain.Shared;

namespace Starbender.RecipeApp.Services.Contracts;

public class RecipeServiceContractsModule : ModuleBase
{
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // Add dependent modules here...
        services.AddModule<RecipeDomainSharedModule>();

        // Add service registrations here...
        services.AddAutoMapper(typeof(RecipeAppServiceContractsMapper));

        return services;
    }
}
