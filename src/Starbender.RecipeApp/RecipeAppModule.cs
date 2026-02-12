using Starbender.BlobStorage;
using Starbender.Core;
using Starbender.Core.Extensions;
using Starbender.RecipeApp.Services;

namespace Starbender.RecipeApp;

public class RecipeAppModule : ModuleBase
{
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // Add dependent modules here...
        services.AddModule<StarbenderCoreModule>();
        services.AddModule<StarbenderBlobStorageModule>();
        services.AddModule<RecipeServicesModule>();

        // Add local service registrations here...

        return services;
    }
}
