using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Starbender.BlobStorage.Contracts;
using Starbender.BlobStorage.Entities;
using Starbender.BlobStorage.Options;
using Starbender.Core;

namespace Starbender.BlobStorage.Services;

public class FilesystemContainerProvider : IBlobContainer
{
    public const string RootPathKey = "RootPath";
    public const string ReadOnlyKey = "ReadOnly";

    private readonly BlobContainerOptions _options = null!;
    private readonly ILoggerFactory _logFactory;
    private readonly ILogger _logger;
    private readonly IRepository<BlobMetadata> _metadataRepo;
    private readonly IMapper _mapper;

    public FilesystemContainerProvider(IServiceProvider serviceProvider, BlobContainerOptions options)
    {
        _options = options;
        _logFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = _logFactory.CreateLogger($"{nameof(FilesystemContainerProvider)}[{ContainerId}]");
        _metadataRepo = serviceProvider.GetRequiredService<IRepository<BlobMetadata>>();
        _mapper = serviceProvider.GetRequiredService<IMapper>();
        StoreType = _options.StoreType;
        ContainerId = _options?.ContainerId ?? string.Empty;
        EnsureInitialized();
    }

    public bool IsReadOnly { get; private set; }

    public BlobStoreType StoreType { get; init; }

    public string ContainerId { get; init; }

    private IQueryable<BlobMetadata> Metadata() =>
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

    public async Task<BlobContentDto> GetContentAsync(string blobId, CancellationToken ct = default)
    {
        var path = Path.Combine(_options.Parameters[RootPathKey], blobId);
        var metadata = await Metadata().Where(t => t.BlobId == blobId).FirstOrDefaultAsync(ct);
        var result = _mapper.Map<BlobContentDto>(metadata);

        result.Content = File.Exists(path)
            ? await File.ReadAllBytesAsync(path, ct)
            : throw new Exception($"Blob '{blobId}' not found");

        return result;
    }

    public async Task<BlobMetadataDto> CreateContentAsync(BlobContentCreateDto content, CancellationToken ct = default)
    {
        if (IsReadOnly)
        {
            throw new Exception("Blob container is read only");
        }

        var blobId = Guid.NewGuid().ToString();
        var data = content.Content;

        var path = Path.Combine(_options.Parameters[RootPathKey], blobId);

        if (File.Exists(path))
        {
            throw new Exception($"Content already exists");
        }

        var file = File.Create(path);
        await file.WriteAsync(data, ct);
        file.Close();

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

    public async Task<BlobMetadataDto> UpdateContentAsync(BlobContentUpdateDto content, CancellationToken ct = default)
    {
        if (IsReadOnly)
        {
            throw new Exception("Blob container is read only");
        }
        
        var blobId = content.BlobId;
        var data = content.Content;

        var path = Path.Combine(_options.Parameters[RootPathKey], blobId);

        var metadata = await Metadata().FirstOrDefaultAsync(t=>t.BlobId==blobId);

        // Delete content regardless of if it's registered
        await DeleteContentAsync(blobId, ct);

        if (metadata == null)
        {
            throw new Exception($"Blob '{blobId}' not found");
        }

        metadata.Size = (ulong)data.Length;
        metadata.Checksum= 0; // TODO: add checksum

        var file = File.Create(path);
        await file.WriteAsync(data, ct);
        file.Close();

        metadata = await _metadataRepo.UpdateAsync(metadata, ct);
        var result = _mapper.Map<BlobMetadataDto>(metadata);

        return result;
    }

    public async Task DeleteContentAsync(string blobId, CancellationToken ct = default)
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

        var path = Path.Combine(_options.Parameters[RootPathKey], blobId);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private void EnsureInitialized()
    {
        if (_options.Parameters.TryGetValue(RootPathKey, out var rootPath))
        {
            Directory.CreateDirectory(rootPath);
        }

        IsReadOnly = Convert.ToBoolean(_options.Parameters.GetValueOrDefault(ReadOnlyKey, bool.FalseString));
    }
}
