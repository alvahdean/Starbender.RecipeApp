using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Starbender.RecipeApp.Core;

public abstract class ModuleBase : IModule
{
    public abstract IServiceCollection ConfigureServices(IServiceCollection services);
    public static IConfiguration Configuration { get; set; } = default!; 
}
