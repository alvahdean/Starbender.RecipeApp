using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Starbender.BlobStorage.Contracts;
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

        BlobStoreOptions options= new();

        configuration.GetSection(BlobStoreOptions.DefaultConfigurationKey).Bind(options);

        foreach (var opt in options.Containers)
        {
            if (string.IsNullOrWhiteSpace(opt.Value.ContainerId))
            {
                opt.Value.ContainerId = opt.Key;
            }

            if (!ValidateBlobContainerOptions(opt.Value))
            {
                throw new Exception($"{nameof(BlobStoreOptions)}.Container[{opt.Key}] is invalid");
            }
        }

        services.AddSingleton(options);
        services.AddSingleton(Options.Create(options));
    }

    public void ConfigureContainers(IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<BlobStoreOptions>>().Value;

        services.AddTransient<IBlobContainerFactory, BlobContainerFactory>();

        foreach (var containerOptions in options.Containers.Values)
        {
            switch (containerOptions.StoreType)
            {
                case BlobStoreType.Filesystem:
                    services.AddTransient<IBlobContainer>(sp => new FilesystemContainerProvider(sp, containerOptions));
                    break;
                case BlobStoreType.Azure:
                    services.AddTransient<IBlobContainer>(sp => new AzureBlobContainerProvider(sp, containerOptions));
                    break;

                case BlobStoreType.Unspecified:
                default:
                    throw new NotSupportedException($"Blob Store Type '{containerOptions.StoreType}' is not supported");
            }
        }
    }

    private static bool ValidateBlobContainerOptions(BlobContainerOptions options)
    {
        var storeTypeOk = options.StoreType != BlobStoreType.Unspecified;
        var readOnlyOk = !options.Parameters.ContainsKey("ReadOnly")
            || bool.TryParse(options.Parameters["ReadOnly"], out _);

        var filesystemParamsOk = options.StoreType == BlobStoreType.Filesystem
            && options.Parameters.ContainsKey(FilesystemContainerProvider.RootPathKey)
            && !string.IsNullOrWhiteSpace(options.Parameters[FilesystemContainerProvider.RootPathKey]);

        var azureConnectionOk = options.Parameters.TryGetValue(AzureBlobContainerProvider.ConnectionStringKey, out var connectionString)
            && !string.IsNullOrWhiteSpace(connectionString);

        var azureServiceUriOk = options.Parameters.TryGetValue(AzureBlobContainerProvider.ServiceUriKey, out var serviceUri)
            && !string.IsNullOrWhiteSpace(serviceUri);

        var azureCreateIfNotExistsOk = !options.Parameters.ContainsKey(AzureBlobContainerProvider.CreateIfNotExistsKey)
            || bool.TryParse(options.Parameters[AzureBlobContainerProvider.CreateIfNotExistsKey], out _);

        var azureContainerNameOk = options.Parameters.TryGetValue(AzureBlobContainerProvider.ContainerNameKey, out var containerName)
            ? !string.IsNullOrWhiteSpace(containerName)
            : !string.IsNullOrWhiteSpace(options.ContainerId);

        var azureParamsOk = options.StoreType == BlobStoreType.Azure
            && (azureConnectionOk || azureServiceUriOk)
            && azureCreateIfNotExistsOk
            && azureContainerNameOk;

        var paramsOk = (filesystemParamsOk || azureParamsOk) && readOnlyOk;

        return storeTypeOk && paramsOk;
    }
}
