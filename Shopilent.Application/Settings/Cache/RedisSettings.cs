namespace Shopilent.Application.Settings.Cache;

public class RedisSettings
{
    public string ConnectionString { get; init; } = default!;
    public string InstanceName { get; init; } = default!;
}