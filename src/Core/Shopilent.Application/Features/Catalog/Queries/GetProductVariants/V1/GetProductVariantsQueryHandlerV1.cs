using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Catalog.Errors;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Application.Features.Catalog.Queries.GetProductVariants.V1;

internal sealed class GetProductVariantsQueryHandlerV1 : IQueryHandler<GetProductVariantsQueryV1, IReadOnlyList<ProductVariantDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetProductVariantsQueryHandlerV1> _logger;

    public GetProductVariantsQueryHandlerV1(
        IUnitOfWork unitOfWork,
        ILogger<GetProductVariantsQueryHandlerV1> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<ProductVariantDto>>> Handle(
        GetProductVariantsQueryV1 request, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Verify the product exists
            var product = await _unitOfWork.ProductReader.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} was not found", request.ProductId);
                return Result.Failure<IReadOnlyList<ProductVariantDto>>(
                    Error.NotFound(message: $"Product with ID {request.ProductId} not found"));
            }

            // Get product variants
            var variants = await _unitOfWork.ProductVariantReader.GetByProductIdAsync(request.ProductId, cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} variants for product with ID {ProductId}", 
                variants.Count, request.ProductId);
            
            return Result.Success(variants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving variants for product with ID {ProductId}", request.ProductId);
            
            return Result.Failure<IReadOnlyList<ProductVariantDto>>(
                Error.Failure(
                    code: "ProductVariants.GetFailed",
                    message: $"Failed to retrieve product variants: {ex.Message}"));
        }
    }
}