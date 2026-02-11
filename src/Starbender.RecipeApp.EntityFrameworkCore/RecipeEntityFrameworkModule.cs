using Microsoft.Extensions.DependencyInjection;
using Starbender.RecipeApp.Core;
using Starbender.RecipeApp.Core.Extensions;
using Starbender.RecipeApp.Domain;
using Starbender.RecipeApp.Domain.Shared;
using Starbender.RecipeApp.EntityFrameworkCore.Repositories;

namespace Starbender.RecipeApp.EntityFrameworkCore;

public class RecipeEntityFrameworkModule : ModuleBase
{
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // Add dependent modules here
        services.AddModule<RecipeDomainModule>();

        // Add service registrations here...
        services.AddTransient(typeof(IRepository<,>), typeof(RecipeAppRepository<,>));
        services.AddTransient(typeof(IRepository<>), typeof(RecipeAppRepository<>));
        return services;
    }
}
