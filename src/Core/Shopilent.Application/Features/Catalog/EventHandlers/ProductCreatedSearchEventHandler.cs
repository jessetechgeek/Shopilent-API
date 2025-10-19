using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Application.Abstractions.Search;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Catalog.Events;

namespace Shopilent.Application.Features.Catalog.EventHandlers;

internal sealed class ProductCreatedSearchEventHandler : INotificationHandler<DomainEventNotification<ProductCreatedEvent>>
{
    private readonly ISearchService _searchService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductCreatedSearchEventHandler> _logger;

    public ProductCreatedSearchEventHandler(
        ISearchService searchService,
        IUnitOfWork unitOfWork,
        ILogger<ProductCreatedSearchEventHandler> logger)
    {
        _searchService = searchService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<ProductCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        try
        {
            var productDto = await _unitOfWork.ProductReader.GetDetailByIdAsync(domainEvent.ProductId, cancellationToken);
            if (productDto is null)
            {
                _logger.LogWarning("Product {ProductId} not found for search indexing", domainEvent.ProductId);
                return;
            }

            var searchDocument = ProductSearchDocument.FromProductDto(productDto);
            var result = await _searchService.IndexProductAsync(searchDocument, cancellationToken);

            if (result.IsFailure)
            {
                _logger.LogError("Failed to index product {ProductId} in search: {Error}", 
                    domainEvent.ProductId, result.Error.Message);
            }
            else
            {
                _logger.LogDebug("Successfully indexed product {ProductId} in search", domainEvent.ProductId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing product {ProductId} in search", domainEvent.ProductId);
        }
    }
}