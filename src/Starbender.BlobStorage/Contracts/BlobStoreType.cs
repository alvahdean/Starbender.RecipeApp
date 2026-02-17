namespace Starbender.BlobStorage.Contracts;

public enum BlobStoreType
{
    Unspecified = 0,    // Unset value as default
    Filesystem,         // A file system based store
    Azure,              // Azure based store
}
