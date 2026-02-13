using System;
using System.Collections.Generic;
using System.Text;

namespace Starbender.BlobStorage.Contracts;

public class BlobContentUpdateDto 
{
    public string BlobId { get; set; } = string.Empty;

    /// <summary>
    /// The blob data
    /// </summary>
    public byte[] Content { get; set; } = Array.Empty<byte>();
}
