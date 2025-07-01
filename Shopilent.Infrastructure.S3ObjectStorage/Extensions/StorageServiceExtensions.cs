using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shopilent.Application.Abstractions.S3Storage;
using Shopilent.Application.Settings.S3Storage;
using Shopilent.Infrastructure.S3ObjectStorage.Services;

namespace Shopilent.Infrastructure.S3ObjectStorage.Extensions;

public static class StorageServiceExtensions
{
    public static IServiceCollection AddStorageServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<S3Settings>(configuration.GetSection("S3"));

        var s3Settings = configuration.GetSection("S3").Get<S3Settings>();
        if (s3Settings == null)
            throw new ArgumentException("S3 settings are not configured");

        services.AddScoped<IAmazonS3>(sp =>
        {
            var config = new AmazonS3Config
            {
                ForcePathStyle = s3Settings.ForcePathStyle
            };

            // Configure based on provider
            switch (s3Settings.Provider)
            {
                case S3ProviderSettings.DigitalOcean:
                    config.ServiceURL = s3Settings.ServiceUrl; // e.g., "https://nyc3.digitaloceanspaces.com"
                    config.ForcePathStyle = false; // Digital Ocean uses virtual-hosted-style URLs
                    break;

                case S3ProviderSettings.MinIO:
                    config.ServiceURL = s3Settings.ServiceUrl;
                    config.ForcePathStyle = true;
                    config.Timeout = TimeSpan.FromMinutes(5);
                    break;

                case S3ProviderSettings.Backblaze:
                    config.ServiceURL = s3Settings.ServiceUrl;
                    config.ForcePathStyle = true;
                    config.UseHttp = false;  // Force HTTPS
                    // Add headers to disable checksums
                    config.SignatureVersion = "2";
                    if (s3Settings.ServiceUrl.Contains(s3Settings.DefaultBucket))
                    {
                        // Strip bucket name from ServiceURL if it's there
                        var uri = new Uri(s3Settings.ServiceUrl);
                        var host = uri.Host;
                        config.ServiceURL = $"https://{host}";
                    }
                    break;

                case S3ProviderSettings.AWS:
                default:
                    config.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(s3Settings.Region);
                    break;
            }

            return new AmazonS3Client(
                s3Settings.AccessKey,
                s3Settings.SecretKey,
                config);
        });

        services.AddScoped<IS3StorageService, S3StorageService>();

        return services;
    }
}