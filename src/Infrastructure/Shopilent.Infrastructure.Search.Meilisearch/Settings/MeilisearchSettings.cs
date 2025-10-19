namespace Shopilent.Infrastructure.Search.Meilisearch.Settings;

public class MeilisearchSettings
{
    public const string SectionName = "Meilisearch";
    
    public string Url { get; init; } = "http://localhost:7700";
    public string ApiKey { get; init; } = "";
    public MeilisearchIndexes Indexes { get; init; } = new();
    public int BatchSize { get; init; } = 1000;
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
}

public class MeilisearchIndexes
{
    public string Products { get; init; } = "products";
}