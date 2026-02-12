using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Starbender.Core;

public abstract class ModuleBase : IModule
{
    public abstract IServiceCollection ConfigureServices(IServiceCollection services);
}
