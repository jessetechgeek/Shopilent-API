using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Search;

namespace Shopilent.Infrastructure.Search.Meilisearch.BackgroundServices;

public class SearchIndexInitializationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SearchIndexInitializationService> _logger;

    public SearchIndexInitializationService(
        IServiceProvider serviceProvider,
        ILogger<SearchIndexInitializationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            using var scope = _serviceProvider.CreateScope();
            var searchService = scope.ServiceProvider.GetRequiredService<ISearchService>();

            _logger.LogInformation("Initializing search indexes...");
            await searchService.InitializeIndexesAsync(stoppingToken);
            _logger.LogInformation("Search indexes initialized successfully");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Search index initialization was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize search indexes");
        }
    }
}