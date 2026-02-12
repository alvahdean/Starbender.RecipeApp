using Microsoft.Extensions.DependencyInjection;

namespace Starbender.Core;

public class StarbenderCoreModule : ModuleBase
{
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // Add service registrations here...

        return services;
    }
}
