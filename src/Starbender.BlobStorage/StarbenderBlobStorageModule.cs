using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Starbender.BlobStorage.Contracts;
using Starbender.BlobStorage.Options;
using Starbender.BlobStorage.Services;
using Starbender.Core;
using Starbender.Core.Extensions;

namespace Starbender.BlobStorage;

public class StarbenderBlobStorageModule : ModuleBase
{
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // Add dependent modules here...
        services.AddModule<StarbenderCoreModule>();

        // Add service registrations here...
        ConfigureAutomapping(services);
        ConfigureBlobStorageOptions(services);
        ConfigureContainers(services);

        return services;
    }

    public void ConfigureAutomapping(IServiceCollection services)
    {
        services.AddAutoMapper(typeof(StarbenderBlobStorageMapper));
    }

    public void ConfigureBlobStorageOptions(IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        services.AddOptions<BlobStoreOptions>()
                .Bind(configuration.GetSection(BlobStoreOptions.DefaultConfigurationKey))
                .Validate(ValidateBlobStoreOptions, "Invalid BlobStoreOptions")
                .ValidateOnStart();
    }

    public void ConfigureContainers(IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        var options = new BlobStoreOptions();

        configuration.GetSection(BlobStoreOptions.DefaultConfigurationKey)
            .Bind(options);

        services.AddTransient<IBlobContainerFactory, BlobContainerFactory>();

        foreach (var containerOptions in options.Containers)
        {
            switch (containerOptions.StoreType)
            {
                case BlobStoreType.Filesystem:
                    services.AddTransient<IBlobContainer>(sp => new FilesystemContainerProvider(sp, containerOptions));
                    break;

                case BlobStoreType.Unspecified:
                default:
                    throw new NotSupportedException($"Blob Store Type '{containerOptions.StoreType}' is not supported");
            }
        }
    }

    private static bool ValidateBlobStoreOptions(BlobStoreOptions options) =>
        options.Containers.All(ValidateBlobContainerOptions);

    private static bool ValidateBlobContainerOptions(BlobContainerOptions options)
    {
        var storeTypeOk = options.StoreType != BlobStoreType.Unspecified;
        var paramsOk = options.StoreType == BlobStoreType.Filesystem
                && options.Parameters.ContainsKey("RootPath")
                && !string.IsNullOrWhiteSpace(options.Parameters["RootPath"])
                && (!options.Parameters.ContainsKey("ReadOnly")
                    || bool.TryParse(options.Parameters["ReadOnly"], out bool _));

        return storeTypeOk && paramsOk;
    }
}
