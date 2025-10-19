using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Domain.Common.Results;
using Shopilent.Infrastructure.Cache.Redis.Converter;
using Shopilent.Infrastructure.Cache.Redis.Settings;
using Shopilent.Infrastructure.Cache.Redis.Utilities;
using StackExchange.Redis;

namespace Shopilent.Infrastructure.Cache.Redis.Services;

internal class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly DistributedCacheEntryOptions _defaultOptions;
    private readonly IConnectionMultiplexer _redis;
    private readonly IOptions<RedisSettings> _redisSettings;
    private readonly JsonSerializerSettings _jsonSettings;

    public CacheService(
        IDistributedCache cache,
        IOptions<DistributedCacheEntryOptions> options,
        IConnectionMultiplexer redis,
        IOptions<RedisSettings> redisSettings)
    {
        _cache = cache;
        _defaultOptions = options.Value;
        _redis = redis;
        _redisSettings = redisSettings;
        
        // Configure JSON settings for serialization
        _jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new ResultContractResolver(),
            Converters = new List<JsonConverter>
            {
                new DictionaryJsonConverter(),
                new ResultConverter()
            },
            TypeNameHandling = TypeNameHandling.None,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var cachedValue = await _cache.GetAsync(key, cancellationToken);
        if (cachedValue == null)
            return default;
        
        try
        {
            var serializedValue = Encoding.UTF8.GetString(cachedValue);
            
            // Use our custom JSON settings to deserialize
            var result = JsonConvert.DeserializeObject<T>(serializedValue, _jsonSettings);
            
            // Special handling for DTOs with Dictionary<string, object> properties
            if (result != null)
            {
                // Normalize any Dictionary<string, object> properties
                NormalizeDictionaryProperties(result);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            // If we encounter a deserialization error, remove the invalid cache entry
            await _cache.RemoveAsync(key, cancellationToken);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null,
        CancellationToken cancellationToken = default)
    {
        if (value == null) return;

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration ?? _defaultOptions.AbsoluteExpirationRelativeToNow
        };

        try
        {
            // Normalize any Dictionary<string, object> properties before serialization
            NormalizeDictionaryProperties(value);

            // Use our custom JSON settings to serialize
            string serializedValue = JsonConvert.SerializeObject(value, _jsonSettings);
            
            var serializedBytes = Encoding.UTF8.GetBytes(serializedValue);
            await _cache.SetAsync(key, serializedBytes, options, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log the exception but don't rethrow - failed caching shouldn't break the application
            Console.WriteLine($"Error caching value: {ex.Message}");
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? absoluteExpiration = null,
        CancellationToken cancellationToken = default)
    {
        var cachedValue = await GetAsync<T>(key, cancellationToken);

        // Check if cachedValue is null or (in case of value types) default
        if (!EqualityComparer<T>.Default.Equals(cachedValue, default))
        {
            return cachedValue;
        }

        var value = await factory();

        // Only cache if value is not null/default
        if (!EqualityComparer<T>.Default.Equals(value, default))
        {
            await SetAsync(key, value, absoluteExpiration, cancellationToken);
        }

        return value;
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var cachedValue = await _cache.GetStringAsync(key, cancellationToken);
        return cachedValue != null;
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(pattern))
            return;

        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var keys = server.Keys(pattern: _redisSettings.Value.InstanceName + pattern);

        foreach (var key in keys)
        {
            var keyName = key.ToString().Replace(_redisSettings.Value.InstanceName, "");
            await _cache.RemoveAsync(keyName, cancellationToken);
        }
    }

    public async Task<int> ClearAllAsync(CancellationToken cancellationToken = default)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var keys = server.Keys(pattern: _redisSettings.Value.InstanceName + "*").ToList();

        foreach (var key in keys)
        {
            var keyName = key.ToString().Replace(_redisSettings.Value.InstanceName, "");
            await _cache.RemoveAsync(keyName, cancellationToken);
        }

        return keys.Count;
    }
    
    private void NormalizeDictionaryProperties<T>(T obj)
    {
        if (obj == null)
            return;
            
        var type = obj.GetType();
        
        // Skip for Result<T> types - they should be handled by ResultConverter
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Result<>))
            return;
            
        // Process properties of Dictionary<string, object> type
        var dictionaryProperties = type.GetProperties()
            .Where(p => p.PropertyType == typeof(Dictionary<string, object>) && p.CanRead && p.CanWrite)
            .ToList();
            
        foreach (var prop in dictionaryProperties)
        {
            var value = prop.GetValue(obj) as Dictionary<string, object>;
            if (value != null)
            {
                var normalizedValue = JsonElementHandler.NormalizeJsonElements(value);
                prop.SetValue(obj, normalizedValue);
            }
        }
        
        // Process collection properties
        var collectionProperties = type.GetProperties()
            .Where(p => p.PropertyType != typeof(string) && 
                        p.PropertyType.GetInterfaces().Any(i => i.IsGenericType && 
                                                       i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            .ToList();
            
        foreach (var prop in collectionProperties)
        {
            var collection = prop.GetValue(obj) as System.Collections.IEnumerable;
            if (collection != null)
            {
                foreach (var item in collection)
                {
                    // Recursively normalize any objects in collections
                    if (item != null && !item.GetType().IsPrimitive && item.GetType() != typeof(string))
                    {
                        NormalizeDictionaryProperties(item);
                    }
                }
            }
        }
    }
}