using Microsoft.EntityFrameworkCore;

namespace Starbender.BlobStorage.Services;

public class FilesystemContainerProvider : BlobStorageProviderBase
{
    public const string RootPathKey = "RootPath";

    public FilesystemContainerProvider(IServiceProvider serviceProvider, BlobContainerOptions options)
        : base(serviceProvider, options)
    {
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        if (_options.Parameters.TryGetValue(RootPathKey, out var rootPath))
        {
            Directory.CreateDirectory(rootPath);
        }
    }

    public override async Task<byte[]?> ProviderGetContentAsync(string blobId, CancellationToken ct = default)
    {
        var path = Path.Combine(_options.Parameters[RootPathKey], blobId);
        var metadata = await Metadata().Where(t => t.BlobId == blobId).FirstOrDefaultAsync(ct);

        var result = File.Exists(path)
            ? await File.ReadAllBytesAsync(path, ct)
            : null;

        return result;
    }

    public override async Task<string> ProviderCreateContentAsync(byte[] data, CancellationToken ct = default)
    {
        var blobId = Guid.NewGuid().ToString();

        var path = Path.Combine(_options.Parameters[RootPathKey], blobId);

        if (File.Exists(path))
        {
            throw new Exception($"FIle already exists");
        }

        return blobId;
    }

    public override async Task<string> ProviderUpdateContentAsync(string blobId, byte[] data, CancellationToken ct = default)
    {
        if (IsReadOnly)
        {
            throw new Exception("Blob container is read only");
        }

        var path = Path.Combine(_options.Parameters[RootPathKey], blobId);

        await using (var file = File.Create(path))
        {
            await file.WriteAsync(data, ct);
        }
        
        return blobId;
    }

    public override Task ProviderDeleteContentAsync(string blobId, CancellationToken ct = default)
    {
        if (IsReadOnly)
        {
            throw new Exception("Blob container is read only");
        }

        var path = Path.Combine(_options.Parameters[RootPathKey], blobId);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }
}
