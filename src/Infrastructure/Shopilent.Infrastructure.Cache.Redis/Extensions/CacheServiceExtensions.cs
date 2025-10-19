using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Infrastructure.Cache.Redis.Converter;
using Shopilent.Infrastructure.Cache.Redis.Services;
using Shopilent.Infrastructure.Cache.Redis.Settings;
using StackExchange.Redis;

namespace Shopilent.Infrastructure.Cache.Redis.Extensions;

public static class CacheServiceExtensions
{
    public static IServiceCollection AddCacheServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RedisSettings>(configuration.GetSection("Redis"));

        services.AddStackExchangeRedisCache(options =>
        {
            var redisSettings = configuration.GetSection("Redis").Get<RedisSettings>();
            options.Configuration = redisSettings!.ConnectionString;
            options.InstanceName = redisSettings!.InstanceName;
        });

        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(configuration.GetSection("Redis:ConnectionString").Value!)
        );
        
        services.AddSingleton<ICacheService, CacheService>();
        services.Configure<DistributedCacheEntryOptions>(options =>
        {
            options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
        });

        // AddHealthChecks
        services.AddHealthChecks(configuration);

        return services;
    }

    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var redisSettings = configuration.GetSection("Redis").Get<RedisSettings>();
        services
            .AddHealthChecks()
            .AddRedis(redisSettings.ConnectionString!);

        return services;
    }
}