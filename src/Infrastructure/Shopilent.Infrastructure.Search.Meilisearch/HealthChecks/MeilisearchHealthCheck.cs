using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shopilent.Application.Abstractions.Search;

namespace Shopilent.Infrastructure.Search.Meilisearch.HealthChecks;

public class MeilisearchHealthCheck : IHealthCheck
{
    private readonly ISearchService _searchService;

    public MeilisearchHealthCheck(ISearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var isHealthy = await _searchService.IsHealthyAsync(cancellationToken);
            
            return isHealthy 
                ? HealthCheckResult.Healthy("Meilisearch is responsive")
                : HealthCheckResult.Unhealthy("Meilisearch is not responsive");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Meilisearch health check failed", ex);
        }
    }
}