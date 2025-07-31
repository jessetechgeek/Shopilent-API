using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Search;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Catalog.Events;

namespace Shopilent.Application.Features.Catalog.EventHandlers;

internal sealed class ProductDeletedSearchEventHandler : INotificationHandler<DomainEventNotification<ProductDeletedEvent>>
{
    private readonly ISearchService _searchService;
    private readonly ILogger<ProductDeletedSearchEventHandler> _logger;

    public ProductDeletedSearchEventHandler(
        ISearchService searchService,
        ILogger<ProductDeletedSearchEventHandler> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<ProductDeletedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        try
        {
            var result = await _searchService.DeleteProductAsync(domainEvent.ProductId, cancellationToken);

            if (result.IsFailure)
            {
                _logger.LogError("Failed to delete product {ProductId} from search index: {Error}", 
                    domainEvent.ProductId, result.Error.Message);
            }
            else
            {
                _logger.LogDebug("Successfully deleted product {ProductId} from search index", domainEvent.ProductId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId} from search index", domainEvent.ProductId);
        }
    }
}