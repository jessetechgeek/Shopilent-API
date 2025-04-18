namespace Shopilent.Application.Settings.S3Storage;

public class S3Settings
{
    public string Provider { get; init; } = "AWS"; // AWS, DigitalOcean, MinIO, etc.
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string Region { get; init; } = string.Empty;
    public string DefaultBucket { get; init; } = string.Empty;
    public string ServiceUrl { get; init; } = string.Empty; // Endpoint URL for the service
    public bool ForcePathStyle { get; init; } = true;

    // Digital Ocean Spaces specific settings
    public string SpaceName { get; init; } = string.Empty;

    // MinIO specific settings
    public bool UseSsl { get; init; } = true;
}