using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Starbender.BlobStorage.Services;

public class AzureBlobContainerProvider : BlobStorageProviderBase
{
    public const string ConnectionStringKey = "ConnectionString";
    public const string ServiceUriKey = "ServiceUri";
    public const string ContainerNameKey = "ContainerName";
    public const string CreateIfNotExistsKey = "CreateIfNotExists";

    private readonly BlobContainerClient _containerClient;

    public AzureBlobContainerProvider(IServiceProvider serviceProvider, BlobContainerOptions options)
        : base(serviceProvider, options)
    {
        var containerName = ResolveContainerName();
        _containerClient = CreateContainerClient(containerName);

        EnsureInitializedAsync().GetAwaiter().GetResult();
    }

    private BlobContainerClient CreateContainerClient(string containerName)
    {
        if (_options.Parameters.TryGetValue(ConnectionStringKey, out var connectionString)
            && !string.IsNullOrWhiteSpace(connectionString))
        {
            return new BlobContainerClient(connectionString, containerName);
        }

        if (_options.Parameters.TryGetValue(ServiceUriKey, out var serviceUri)
            && !string.IsNullOrWhiteSpace(serviceUri))
        {
            var serviceClient = new BlobServiceClient(new Uri(serviceUri), new DefaultAzureCredential());
            return serviceClient.GetBlobContainerClient(containerName);
        }

        throw new InvalidOperationException(
            $"Azure blob container '{ContainerId}' requires '{ConnectionStringKey}' or '{ServiceUriKey}' parameter.");
    }

    private string ResolveContainerName()
    {
        if (_options.Parameters.TryGetValue(ContainerNameKey, out var containerName)
            && !string.IsNullOrWhiteSpace(containerName))
        {
            return containerName;
        }

        if (!string.IsNullOrWhiteSpace(ContainerId))
        {
            return ContainerId;
        }

        throw new InvalidOperationException(
            $"Azure blob container requires '{ContainerNameKey}' parameter when '{nameof(ContainerId)}' is empty.");
    }

    private async Task EnsureInitializedAsync()
    {

        var createIfNotExists = Convert.ToBoolean(
            _options.Parameters.GetValueOrDefault(CreateIfNotExistsKey, bool.TrueString));

        if (createIfNotExists)
        {
            await _containerClient.CreateIfNotExistsAsync(cancellationToken: CancellationToken.None);
        }
    }

    public override async Task<byte[]?> ProviderGetContentAsync(string blobId, CancellationToken ct = default)
    {
        var blobClient = _containerClient.GetBlobClient(blobId);
        var exists = await blobClient.ExistsAsync(ct);

        if (!exists.Value)
        {
            return null;
        }

        var response = await blobClient.DownloadContentAsync(ct);
        return response.Value.Content.ToArray();
    }

    public override async Task<string> ProviderCreateContentAsync(byte[] content, CancellationToken ct = default)
    {
        for (var i = 0; i < 5; i++)
        {
            var blobId = Guid.NewGuid().ToString();
            var blobClient = _containerClient.GetBlobClient(blobId);

            try
            {
                await blobClient.UploadAsync(
                    BinaryData.FromBytes(content),
                    options: new BlobUploadOptions(),
                    cancellationToken: ct);
                return blobId;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 409)
            {
                // Rare name collision. Try again with a new id.
            }
        }

        throw new InvalidOperationException("Could not generate a unique blob id after multiple attempts.");
    }

    public override async Task<string> ProviderUpdateContentAsync(string blobId, byte[] content, CancellationToken ct = default)
    {
        var blobClient = _containerClient.GetBlobClient(blobId);
        await blobClient.UploadAsync(
            BinaryData.FromBytes(content),
            overwrite: true,
            cancellationToken: ct);

        return blobId;
    }

    public override async Task ProviderDeleteContentAsync(string blobId, CancellationToken ct = default)
    {
        var blobClient = _containerClient.GetBlobClient(blobId);
        await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
    }
}
