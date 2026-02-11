using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Starbender.RecipeApp.Core.Extensions;

public static class AppBuilderExtensions
{
    public static IHostApplicationBuilder InitializeAppModules(this IHostApplicationBuilder builder)
    {
        ModuleBase.Configuration = builder.Configuration;

        var entryAssembly = Assembly.GetEntryAssembly();
        var moduleType = entryAssembly?.DefinedTypes.FirstOrDefault(t =>
            t.IsClass
            && !t.IsAbstract
            && t.ImplementedInterfaces.Contains(typeof(IModule)))
            ?? throw new Exception("No IModule found in startup assembly");

        var module = Activator.CreateInstance(moduleType) as IModule
            ?? throw new Exception($"Can't create instance of {moduleType.FullName}");

        module.ConfigureServices(builder.Services);

        return builder;
    }
}
