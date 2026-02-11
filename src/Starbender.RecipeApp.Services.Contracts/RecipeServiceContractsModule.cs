using Microsoft.Extensions.DependencyInjection;
using Starbender.RecipeApp.Core;
using Starbender.RecipeApp.Core.Extensions;
using Starbender.RecipeApp.Domain.Shared;

namespace Starbender.RecipeApp.Services.Contracts;

public class RecipeServiceContractsModule : ModuleBase
{
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // Add service registrations here...
        services.AddModule<RecipeDomainSharedModule>();
        services.AddAutoMapper(typeof(AutoMapperService));
        return services;
    }
}
