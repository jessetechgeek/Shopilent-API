using Shopilent.Domain.Common.Results;

namespace Shopilent.Application.Abstractions.S3Storage;

public interface IS3StorageService
{
    Task<Result<string>> UploadFileAsync(
        string bucketName, 
        string key, 
        Stream fileStream, 
        string contentType,
        IDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default);

    Task<Result<Stream>> DownloadFileAsync(
        string bucketName,
        string key,
        CancellationToken cancellationToken = default);

    Task<Result<bool>> DeleteFileAsync(
        string bucketName,
        string key,
        CancellationToken cancellationToken = default);

    Task<Result<bool>> FileExistsAsync(
        string bucketName,
        string key,
        CancellationToken cancellationToken = default);

    Task<Result<string>> GetPresignedUrlAsync(
        string bucketName,
        string key,
        TimeSpan expiry,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<S3ObjectInfo>>> ListFilesAsync(
        string bucketName,
        string? prefix = null,
        CancellationToken cancellationToken = default);
}

public class S3ObjectInfo
{
    public string Key { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
    public string ETag { get; set; } = string.Empty;
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}