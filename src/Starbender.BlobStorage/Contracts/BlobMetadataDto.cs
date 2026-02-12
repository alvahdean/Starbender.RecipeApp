using Starbender.Core;

namespace Starbender.BlobStorage.Contracts;

public class BlobMetadataDto : IDto<int>
{
    /// <summary>
    /// Internal Id for the blob
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The type of blob store
    /// </summary>
    public BlobStoreType StoreType { get; set; }

    /// <summary>
    /// The instance ID of the provider (StoreType)
    /// </summary>
    /// <remarks>
    /// There may be multiple container for the same store type
    /// and the container ID must be unique within the store type
    /// </remarks>
    public string ContainerId { get; set; } = string.Empty;

    /// <summary>
    /// The location of the blob relative to the root of the specified
    /// store instance. The format is store provider dependent.
    /// </summary>
    public string BlobId { get; set; } = string.Empty;

    /// <summary>
    /// The size in bytes of the content
    /// </summary>
    public ulong Size { get; set; }

    /// <summary>
    /// The CRC32 checksum for the content of the content
    /// </summary>
    public uint Checksum { get; set; }
}
