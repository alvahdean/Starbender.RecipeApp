using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Starbender.BlobStorage.Contracts;
using Starbender.BlobStorage.Options;

namespace Starbender.BlobStorage.Services;

public class BlobContainerFactory : IBlobContainerFactory
{
    private readonly BlobStoreOptions _options = new();
    private readonly List<IBlobContainer> _containers = new();
    private readonly ILoggerFactory _logFactory = null!;
    private readonly ILogger _logger = null!;

    public BlobContainerFactory(
        IOptions<BlobStoreOptions> options,
        IEnumerable<IBlobContainer> containers,
        ILoggerFactory logFactory)
    {
        _containers = containers.ToList();
        _options = options.Value;
        _logFactory = logFactory;
        _logger = _logFactory.CreateLogger<BlobContainerFactory>();
    }

    public IReadOnlyList<BlobStoreType> GetContainerTypes()
    {
        var result = _containers
            .Select(t => t.StoreType)
            .Distinct()
            .ToList();

        return result;
    }

    public IReadOnlyList<string> GetContainerIds(BlobStoreType storeType)
    {
        var result = _containers
            .Where(t => t.StoreType == storeType)
            .Select(t => t.ContainerId)
            .Distinct()
            .ToList();

        return result;
    }

    public IBlobContainer? GetContainer(BlobStoreType storeType, string containerId)
    {
        var result = _containers
            .Where(t => t.StoreType == storeType && t.ContainerId == containerId)
            .FirstOrDefault();

        return result;
    }

    public IBlobContainer? GetDefaultContainer(BlobStoreType storeType)
    {
        var typeContainers = _containers
            .Where(t => t.StoreType == storeType)
            .ToList();

        var result = typeContainers
            .Where(t => string.IsNullOrWhiteSpace(t.ContainerId))
            .FirstOrDefault();

        result ??= typeContainers.FirstOrDefault();

        return result;
    }
}
