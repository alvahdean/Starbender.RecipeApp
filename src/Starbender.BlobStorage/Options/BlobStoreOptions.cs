using Starbender.BlobStorage.Contracts;

namespace Starbender.BlobStorage.Options;

/// <summary>
/// Options for configuring blob storage containers
/// </summary>
public class BlobStoreOptions
{
    public const string DefaultConfigurationKey = "BlobStorage";
    public BlobContainerOptions[] Containers { get; set; } = Array.Empty<BlobContainerOptions>();
}

/// <summary>
/// The definition for a blob storage container that the application should provide access to
/// </summary>
/// <example>
/// {
///   "StoreType": "Filesystem",
///   "ContainerId": "Images",
///   "Parameters": {
///     "RootPath": "C:/tmp/ImageStore",
///     "ReadOnly": "true"
///     }
/// }
/// </example>
public class BlobContainerOptions
{
    /// <summary>
    /// The type of blob storage
    /// </summary>
    public BlobStoreType StoreType { get; set; } = BlobStoreType.Unspecified;

    /// <summary>
    /// The ID for this container. Often this is just a name but can be anything.
    /// - Must be unique across a StoreType.
    /// - Can be empty to indicate the "default" instance for the type.
    /// </summary>
    public string ContainerId { get; set; } = string.Empty;

    /// <summary>
    /// Additional provider/storetype specific configuration options. See the provider type for supported options.
    /// </summary>
    public Dictionary<string, string> Parameters { get; set; } = new();
}

