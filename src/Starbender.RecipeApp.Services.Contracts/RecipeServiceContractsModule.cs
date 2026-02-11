using Microsoft.Extensions.DependencyInjection;
using Starbender.RecipeApp.Core;
using Starbender.RecipeApp.Core.Extensions;
using Starbender.RecipeApp.Domain.Shared;

namespace Starbender.RecipeApp.Services.Contracts;

public class RecipeServiceContractsModule : ModuleBase
{
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // Add dependent modules here...
        services.AddModule<RecipeDomainSharedModule>();

        // Add service registrations here...
        services.AddAutoMapper(typeof(AutoMapperService));

        return services;
    }
}
