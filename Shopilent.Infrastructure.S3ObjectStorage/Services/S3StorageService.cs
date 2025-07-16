using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shopilent.Application.Abstractions.S3Storage;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;
using Shopilent.Infrastructure.S3ObjectStorage.Settings;

namespace Shopilent.Infrastructure.S3ObjectStorage.Services;

public class S3StorageService : IS3StorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3StorageService> _logger;
    private readonly S3Settings _settings;

    public S3StorageService(
        IAmazonS3 s3Client,
        IOptions<S3Settings> settings,
        ILogger<S3StorageService> logger)
    {
        _s3Client = s3Client;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<Result<string>> UploadFileAsync(
        string bucketName,
        string key,
        Stream fileStream,
        string contentType,
        IDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var transferUtility = new TransferUtility(_s3Client);
            var request = new TransferUtilityUploadRequest
            {
                BucketName = bucketName,
                Key = key,
                InputStream = fileStream,
                ContentType = contentType
            };

            if (metadata != null)
            {
                foreach (var (metaKey, value) in metadata)
                {
                    request.Metadata.Add(metaKey, value);
                }
            }

            await transferUtility.UploadAsync(request, cancellationToken);

            string url;
            switch (_settings.Provider?.ToUpperInvariant())
            {
                case "DIGITALOCEAN":
                    url = $"https://{bucketName}.{new Uri(_settings.ServiceUrl).Host}/{key}";
                    break;

                case "BACKBLAZE":
                    var serviceUri = new Uri(_settings.ServiceUrl);
                    url = $"https://{serviceUri.Host}/{key}";
                    break;

                default:
                    url = $"{_settings.ServiceUrl}/{key}";
                    break;
            }

            return Result.Success(url);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to S3. Bucket: {Bucket}, Key: {Key}", bucketName, key);
            return Result.Failure<string>(Error.Failure(message: $"Failed to upload file: {ex.Message}"));
        }
    }

    public async Task<Result<Stream>> DownloadFileAsync(
        string bucketName,
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            var response = await _s3Client.GetObjectAsync(request, cancellationToken);
            return Result.Success(response.ResponseStream);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Result.Failure<Stream>(Error.NotFound(message: "File not found"));
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from S3. Bucket: {Bucket}, Key: {Key}", bucketName, key);
            return Result.Failure<Stream>(Error.Failure(message: $"Failed to download file: {ex.Message}"));
        }
    }

    public async Task<Result<bool>> DeleteFileAsync(
        string bucketName,
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(request, cancellationToken);
            return Result.Success(true);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from S3. Bucket: {Bucket}, Key: {Key}", bucketName, key);
            return Result.Failure<bool>(Error.Failure(message: $"Failed to delete file: {ex.Message}"));
        }
    }

    public async Task<Result<bool>> FileExistsAsync(
        string bucketName,
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = bucketName,
                Key = key
            };

            await _s3Client.GetObjectMetadataAsync(request, cancellationToken);
            return Result.Success(true);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Result.Success(false);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence in S3. Bucket: {Bucket}, Key: {Key}", bucketName, key);
            return Result.Failure<bool>(Error.Failure(message: $"Failed to check file existence: {ex.Message}"));
        }
    }

    public async Task<Result<string>> GetPresignedUrlAsync(
        string bucketName,
        string key,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = key,
                Expires = DateTime.UtcNow.Add(expiry)
            };

            var url = _s3Client.GetPreSignedURL(request);
            if (_settings.Provider == "MinIO")
            {
                url = url.Replace(_settings.ServiceUrl.Replace("http", "https"), "http://localhost:9858");
            }

            return Result.Success(url);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned URL. Bucket: {Bucket}, Key: {Key}", bucketName, key);
            return Result.Failure<string>(Error.Failure(message: $"Failed to generate presigned URL: {ex.Message}"));
        }
    }

    public async Task<Result<IEnumerable<S3ObjectInfo>>> ListFilesAsync(
        string bucketName,
        string? prefix = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = prefix
            };

            var response = await _s3Client.ListObjectsV2Async(request, cancellationToken);
            var objects = response.S3Objects.Select(obj => new S3ObjectInfo
            {
                Key = obj.Key,
                Size = obj.Size,
                LastModified = obj.LastModified,
                ETag = obj.ETag
            });

            return Result.Success(objects);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Error listing files in S3. Bucket: {Bucket}, Prefix: {Prefix}", bucketName, prefix);
            return Result.Failure<IEnumerable<S3ObjectInfo>>(
                Error.Failure(message: $"Failed to list files: {ex.Message}"));
        }
    }
}