using AutoMapper;
using Starbender.BlobStorage.Contracts;
using Starbender.BlobStorage.Entities;

namespace Starbender.BlobStorage;

public class StarbenderBlobStorageMapper : Profile
{
    public StarbenderBlobStorageMapper()
    {

        CreateMap<BlobMetadata, BlobMetadataDto>()
            .ReverseMap()
            .ForMember(t=>t.Id,o=>o.Ignore());

        CreateMap<BlobMetadata, BlobContentDto>();
    }
}
