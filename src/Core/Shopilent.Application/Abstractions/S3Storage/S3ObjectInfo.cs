namespace Shopilent.Application.Abstractions.S3Storage;

public class S3ObjectInfo
{
    public string Key { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
    public string ETag { get; set; } = string.Empty;
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}