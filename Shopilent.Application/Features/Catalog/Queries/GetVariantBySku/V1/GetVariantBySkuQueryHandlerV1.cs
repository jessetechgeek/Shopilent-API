using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Application.Features.Catalog.Queries.GetVariantBySku.V1;

internal sealed class GetVariantBySkuQueryHandlerV1 : IQueryHandler<GetVariantBySkuQueryV1, ProductVariantDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetVariantBySkuQueryHandlerV1> _logger;

    public GetVariantBySkuQueryHandlerV1(
        IUnitOfWork unitOfWork,
        ILogger<GetVariantBySkuQueryHandlerV1> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ProductVariantDto>> Handle(
        GetVariantBySkuQueryV1 request, 
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Sku))
            {
                return Result.Failure<ProductVariantDto>(
                    Error.Validation(message: "SKU cannot be empty"));
            }

            var variant = await _unitOfWork.ProductVariantReader.GetBySkuAsync(request.Sku, cancellationToken);
            
            if (variant == null)
            {
                _logger.LogWarning("Product variant with SKU {Sku} was not found", request.Sku);
                return Result.Failure<ProductVariantDto>(
                    Error.NotFound(message: $"Product variant with SKU {request.Sku} not found"));
            }

            _logger.LogInformation("Retrieved product variant with SKU {Sku}", request.Sku);
            return Result.Success(variant);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product variant with SKU {Sku}", request.Sku);
            
            return Result.Failure<ProductVariantDto>(
                Error.Failure(
                    code: "ProductVariant.GetBySkuFailed", 
                    message: $"Failed to retrieve product variant: {ex.Message}"));
        }
    }
}