using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Application.Abstractions.Search;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Catalog.Events;

namespace Shopilent.Application.Features.Catalog.EventHandlers;

internal sealed class ProductUpdatedSearchEventHandler : INotificationHandler<DomainEventNotification<ProductUpdatedEvent>>
{
    private readonly ISearchService _searchService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductUpdatedSearchEventHandler> _logger;

    public ProductUpdatedSearchEventHandler(
        ISearchService searchService,
        IUnitOfWork unitOfWork,
        ILogger<ProductUpdatedSearchEventHandler> logger)
    {
        _searchService = searchService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<ProductUpdatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        try
        {
            var productDto = await _unitOfWork.ProductReader.GetDetailByIdAsync(domainEvent.ProductId, cancellationToken);
            if (productDto is null)
            {
                _logger.LogWarning("Product {ProductId} not found for search re-indexing", domainEvent.ProductId);
                return;
            }

            var searchDocument = ProductSearchDocument.FromProductDto(productDto);
            var result = await _searchService.IndexProductAsync(searchDocument, cancellationToken);

            if (result.IsFailure)
            {
                _logger.LogError("Failed to re-index product {ProductId} in search: {Error}", 
                    domainEvent.ProductId, result.Error.Message);
            }
            else
            {
                _logger.LogDebug("Successfully re-indexed product {ProductId} in search", domainEvent.ProductId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error re-indexing product {ProductId} in search", domainEvent.ProductId);
        }
    }
}