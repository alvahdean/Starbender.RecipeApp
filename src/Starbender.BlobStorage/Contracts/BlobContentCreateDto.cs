using System;
using System.Collections.Generic;
using System.Text;

namespace Starbender.BlobStorage.Contracts;

public class BlobContentCreateDto 
{
    /// <summary>
    /// The MIME Content-Type of the data
    /// </summary>
    public string? ContentType {  get; set; }

    public byte[] Content { get; set; } = Array.Empty<byte>();
}