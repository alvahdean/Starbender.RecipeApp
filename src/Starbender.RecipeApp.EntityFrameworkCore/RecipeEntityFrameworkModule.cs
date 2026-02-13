using Microsoft.Extensions.DependencyInjection;
using Starbender.Core;
using Starbender.Core.Extensions;
using Starbender.RecipeApp.Domain;
using Starbender.RecipeApp.Domain.Shared.Entities;
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
        services.AddTransient<IRepository<Recipe>, RecipeRepository>();
        return services;
    }
}
