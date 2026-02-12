using Starbender.BlobStorage;
using Starbender.Core;
using Starbender.Core.Extensions;
using Starbender.RecipeApp.Blazor;
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
        services.AddModule<RecipeAppBlazorModule>();

        // Add local service registrations here...

        return services;
    }
}
