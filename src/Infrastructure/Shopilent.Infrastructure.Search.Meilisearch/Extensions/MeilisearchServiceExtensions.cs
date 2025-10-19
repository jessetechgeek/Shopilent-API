using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shopilent.Application.Abstractions.Search;
using Shopilent.Infrastructure.Search.Meilisearch.BackgroundServices;
using Shopilent.Infrastructure.Search.Meilisearch.HealthChecks;
using Shopilent.Infrastructure.Search.Meilisearch.Services;
using Shopilent.Infrastructure.Search.Meilisearch.Settings;

namespace Shopilent.Infrastructure.Search.Meilisearch.Extensions;

public static class MeilisearchServiceExtensions
{
    public static IServiceCollection AddMeilisearch(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MeilisearchSettings>(configuration.GetSection(MeilisearchSettings.SectionName));
        
        services.AddScoped<ISearchService, MeilisearchService>();
        
        services.AddHealthChecks()
            .AddCheck<MeilisearchHealthCheck>("meilisearch", HealthStatus.Degraded);
        
        services.AddHostedService<SearchIndexInitializationService>();
        
        return services;
    }
}