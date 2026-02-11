using Microsoft.Extensions.DependencyInjection;
using Starbender.RecipeApp.Core;
using Starbender.RecipeApp.Core.Extensions;
using Starbender.RecipeApp.Domain.Shared;

namespace Starbender.RecipeApp.Domain;

public class RecipeDomainModule : ModuleBase
{
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.AddModule<RecipeDomainSharedModule>();
        services.AddTransient(typeof(IRepository<,>), typeof(EfRepository<,>));
        services.AddTransient(typeof(IRepository<>), typeof(EfRepository<>));

        return services;
    }
}
