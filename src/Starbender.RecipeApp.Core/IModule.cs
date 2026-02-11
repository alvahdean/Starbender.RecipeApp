using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Starbender.RecipeApp.Core;

public interface IModule
{
    IServiceCollection ConfigureServices(IServiceCollection services);
}
