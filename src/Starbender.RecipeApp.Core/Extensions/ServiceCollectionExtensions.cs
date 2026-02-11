using Microsoft.Extensions.DependencyInjection;

namespace Starbender.RecipeApp.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddModule<T>(this IServiceCollection services) 
        where T : class, IModule
    {
        var module = Activator.CreateInstance<T>();
        module.ConfigureServices(services);
        return services; 
    }
}
