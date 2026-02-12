using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Starbender.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddModule<T>(this IServiceCollection services) 
        where T : class, IModule
    {
        var module = Activator.CreateInstance<T>();
        module.ConfigureServices(services);
        return services; 
    }
    public static IServiceCollection InitializeAppModules(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        var entryAssembly = Assembly.GetEntryAssembly();
        var moduleType = entryAssembly?.DefinedTypes.FirstOrDefault(t =>
            t.IsClass
            && !t.IsAbstract
            && t.ImplementedInterfaces.Contains(typeof(IModule)))
            ?? throw new Exception("No IModule found in startup assembly");

        var module = Activator.CreateInstance(moduleType) as IModule
            ?? throw new Exception($"Can't create instance of {moduleType.FullName}");

        module.ConfigureServices(services);

        return services;
    }
}
