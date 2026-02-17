using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Starbender.BlobStorage.Contracts;
using Starbender.BlobStorage.Entities;
using Starbender.BlobStorage.Options;
using Starbender.Core;

namespace Starbender.BlobStorage.Services;

public abstract class BlobStorageProviderBase : IBlobContainer
{
    public const string ReadOnlyKey = "ReadOnly";

    protected readonly BlobContainerOptions _options = null!;
    protected readonly ILoggerFactory _logFactory;
    protected readonly ILogger _logger;
    protected readonly IRepository<BlobMetadata> _metadataRepo;
    protected readonly IMapper _mapper;

    public BlobStorageProviderBase(IServiceProvider serviceProvider, BlobContainerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = _logFactory.CreateLogger($"{GetType().Name}[{ContainerId}]");
        _metadataRepo = serviceProvider.GetRequiredService<IRepository<BlobMetadata>>();
        _mapper = serviceProvider.GetRequiredService<IMapper>();
        StoreType = _options.StoreType;
        ContainerId = _options.ContainerId ?? string.Empty;
        IsReadOnly = Convert.ToBoolean(_options.Parameters.GetValueOrDefault(ReadOnlyKey, bool.FalseString));
    }

    public bool IsReadOnly { get; protected set; }

    public BlobStoreType StoreType { get; protected set; }

    public string ContainerId { get; protected set; }

    protected IQueryable<BlobMetadata> Metadata() =>
        _metadataRepo.Query(t => t.StoreType == StoreType);

    public async Task<IReadOnlyList<BlobMetadataDto>> GetAllMetadataAsync(CancellationToken ct = default)
    {
        var data = await Metadata().ToListAsync(ct);
        var result = _mapper.Map<IEnumerable<BlobMetadataDto>>(data);

        return result.ToList();
    }

    public async Task<BlobMetadataDto?> GetMetadataAsync(string blobId, CancellationToken ct = default)
    {
        var data = await Metadata().Where(t => t.BlobId == blobId).FirstOrDefaultAsync(ct);

        var result = _mapper.Map<BlobMetadataDto>(data);

        return result;
    }

    public virtual async Task<BlobContentDto?> GetContentAsync(string blobId, CancellationToken ct = default)
    {
        var metadata = await Metadata().Where(t => t.BlobId == blobId).FirstOrDefaultAsync(ct);

        var result = _mapper.Map<BlobContentDto>(metadata);

        if(result!=null)
        {
            result.Content = await ProviderGetContentAsync(blobId, ct) ?? Array.Empty<byte>();
        }

        return result;
    }

    public virtual async Task<BlobMetadataDto> CreateContentAsync(BlobContentCreateDto content, CancellationToken ct = default)
    {
        if (IsReadOnly)
        {
            throw new Exception("Blob container is read only");
        }

        var data = content.Content;

        var blobId = await ProviderCreateContentAsync(data, ct);

        var metadata = await _metadataRepo.CreateAsync(new BlobMetadata()
        {
            BlobId = blobId,
            StoreType = StoreType,
            ContainerId = ContainerId,
            Size = (ulong)data.Length,
            Checksum = 0
        }, ct);

        var result = _mapper.Map<BlobMetadataDto>(metadata);

        return result;
    }

    public virtual async Task<BlobMetadataDto> UpdateContentAsync(BlobContentUpdateDto content, CancellationToken ct = default)
    {
        if (IsReadOnly)
        {
            throw new Exception("Blob container is read only");
        }

        var blobId = content.BlobId;
        var data = content.Content;

        var blobPk = (await Metadata().FirstOrDefaultAsync(t => t.BlobId == blobId, ct))?.Id ?? 0;

        // Resolve to the tracked instance if one already exists in the DbContext.
        var metadata = await _metadataRepo.GetAsync(blobPk, ct)
            ?? throw new Exception($"Blob '{blobId}' not found");

        metadata.Size = (ulong)data.Length;
        metadata.Checksum = 0; // TODO: add checksum

        metadata.BlobId = await ProviderUpdateContentAsync(blobId, data, ct);

        metadata = await _metadataRepo.UpdateAsync(metadata, ct);

        var result = _mapper.Map<BlobMetadataDto>(metadata);

        return result;
    }

    public virtual async Task DeleteContentAsync(string blobId, CancellationToken ct = default)
    {
        if (IsReadOnly)
        {
            throw new Exception("Blob container is read only");
        }

        var metadata = await GetMetadataAsync(blobId, ct);
        if (metadata != null)
        {
            await _metadataRepo.DeleteAsync(metadata.Id, ct);
        }

        // Call provider delete method 
        await ProviderDeleteContentAsync(blobId, ct);
    }

    /// <summary>
    /// Get the byte data for the specified blobId
    /// </summary>
    /// <param name="blobId"></param>
    /// <param name="ct"></param>
    public abstract Task<byte[]?> ProviderGetContentAsync(string blobId, CancellationToken ct = default);

    /// <summary>
    /// Create a new blob in the provider and return a unique id
    /// </summary>
    /// <param name="content"></param>
    /// <param name="ct"></param>
    public abstract Task<string> ProviderCreateContentAsync(byte[] content, CancellationToken ct = default);

    /// <summary>
    /// Update the blob data in the provider fro the spcified blobId and return the blobId
    /// </summary>
    /// <param name="blobId"></param>
    /// <param name="content"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public abstract Task<string> ProviderUpdateContentAsync(string blobId, byte[] content, CancellationToken ct = default);

    /// <summary>
    /// Delete the blob in the provider
    /// </summary>
    /// <param name="blobId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public abstract Task ProviderDeleteContentAsync(string blobId, CancellationToken ct = default);
}
