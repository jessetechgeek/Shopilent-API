using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Application.Abstractions.Search;

using Shopilent.Application.Common.Models;
using Shopilent.Domain.Catalog.Events;

namespace Shopilent.Application.Features.Catalog.EventHandlers;

internal sealed class ProductVariantUpdatedSearchEventHandler : INotificationHandler<DomainEventNotification<ProductVariantUpdatedEvent>>
{
    private readonly ISearchService _searchService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductVariantUpdatedSearchEventHandler> _logger;

    public ProductVariantUpdatedSearchEventHandler(
        ISearchService searchService,
        IUnitOfWork unitOfWork,
        ILogger<ProductVariantUpdatedSearchEventHandler> logger)
    {
        _searchService = searchService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<ProductVariantUpdatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        try
        {
            // Get the product that contains this variant
            var product = await _unitOfWork.ProductWriter.GetByIdAsync(domainEvent.ProductId, cancellationToken);
            if (product is null)
            {
                _logger.LogWarning("Product {ProductId} not found for search re-indexing after variant update", domainEvent.ProductId);
                return;
            }

            var searchDocument = ProductSearchDocument.FromProduct(product);
            var result = await _searchService.IndexProductAsync(searchDocument, cancellationToken);

            if (result.IsFailure)
            {
                _logger.LogError("Failed to re-index product {ProductId} in search after variant update: {Error}", 
                    domainEvent.ProductId, result.Error.Message);
            }
            else
            {
                _logger.LogDebug("Successfully re-indexed product {ProductId} in search after variant {VariantId} update", 
                    domainEvent.ProductId, domainEvent.VariantId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error re-indexing product {ProductId} in search after variant update", domainEvent.ProductId);
        }
    }
}