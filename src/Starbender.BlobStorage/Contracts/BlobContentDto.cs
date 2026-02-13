using System;
using System.Collections.Generic;
using System.Text;

namespace Starbender.BlobStorage.Contracts;

public class BlobContentDto : BlobMetadataDto
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
}
