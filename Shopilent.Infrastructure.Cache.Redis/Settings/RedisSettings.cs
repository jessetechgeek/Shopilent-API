namespace Shopilent.Infrastructure.Cache.Redis.Settings;

public class RedisSettings
{
    public string ConnectionString { get; init; } = default!;
    public string InstanceName { get; init; } = default!;
}