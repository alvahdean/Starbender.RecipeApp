namespace Starbender.BlobStorage.Contracts;

public interface IBlobContainer
{
    /// <summary>
    /// The type of blob store
    /// </summary>
    BlobStoreType StoreType { get; }

    /// <summary>
    /// The ID of the container within the StoreType
    /// </summary>
    /// <remarks>
    /// There may be multiple containers configured for a given BlobStoreType
    /// </remarks>
    string ContainerId { get; }

    /// <summary>
    /// Retrieves the metadata for all blobs in the container
    /// </summary>
    Task<IReadOnlyList<BlobMetadataDto>> GetAllMetadataAsync(CancellationToken ct);

    /// <summary>
    /// Retrieves the metadata for a specific blob in the container
    /// </summary>
    /// <param name="blobId">The blobId to lookup</param>
    Task<BlobMetadataDto?> GetMetadataAsync(string blobId, CancellationToken ct);

    /// <summary>
    /// Retrieves the byte content of the specified blob
    /// </summary>
    /// <param name="blobId">The ID of the blob to return</param>
    /// <returns>The byte contents of the blob</returns>
    Task<BlobContentDto?> GetContentAsync(string blobId, CancellationToken ct);

    /// <summary>
    /// Creates a new blob entry and returns the ID of the entry for later retrieval
    /// </summary>
    /// <param name="content">The data to store</param>
    /// <returns>An ID for the newly stored blob</returns>
    Task<BlobContentDto> CreateContentAsync(BlobContentDto content, CancellationToken ct);

    /// <summary>
    /// Updates an existing blob entry contents
    /// </summary>
    /// <param name="blobId">The id of the blob to update</param>
    /// <param name="content">The updated data store</param>
    Task<BlobContentDto> UpdateContentAsync(BlobContentDto content, CancellationToken ct);

    /// <summary>
    /// Deletes an existing blob with the specified ID
    /// </summary>
    /// <param name="blobId">The ID of the blob to delete</param>
    Task DeleteContentAsync(string blobId, CancellationToken ct); 
}
