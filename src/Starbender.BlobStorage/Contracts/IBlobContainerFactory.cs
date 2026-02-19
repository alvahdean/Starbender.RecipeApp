namespace Starbender.BlobStorage.Contracts;

public interface IBlobContainerFactory
{
    /// <summary>
    /// Get a list of registered container store types
    /// </summary>
    IReadOnlyList<BlobStoreType> GetContainerTypes();

    /// <summary>
    /// Gets a list of containerIds for the specified store type
    /// </summary>
    /// <param name="storeType"></param>
    IReadOnlyList<string> GetContainerIds(BlobStoreType storeType);

    /// <summary>
    /// Gets the "default" container for the specified store type
    /// </summary>
    /// <param name="storeType"></param>
    IBlobContainer? GetDefaultContainer(BlobStoreType storeType);

    /// <summary>
    /// Gets the container for the specified containerId
    /// </summary>
    /// <param name="storeType"></param>
    IBlobContainer? GetContainer(string containerId);
}
