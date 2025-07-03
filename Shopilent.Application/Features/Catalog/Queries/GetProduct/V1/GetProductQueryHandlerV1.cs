using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Catalog.Errors;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Application.Features.Catalog.Queries.GetProduct.V1;

internal sealed class GetProductQueryHandlerV1 : IQueryHandler<GetProductQueryV1, ProductDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetProductQueryHandlerV1> _logger;

    public GetProductQueryHandlerV1(
        IUnitOfWork unitOfWork,
        ILogger<GetProductQueryHandlerV1> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ProductDetailDto>> Handle(GetProductQueryV1 request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _unitOfWork.ProductReader.GetDetailByIdAsync(request.Id, cancellationToken);
            
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} was not found", request.Id);
                return Result.Failure<ProductDetailDto>(ProductErrors.NotFound(request.Id));
            }

            _logger.LogInformation("Retrieved product with ID {ProductId}", request.Id);
            return Result.Success(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product with ID {ProductId}", request.Id);
            
            return Result.Failure<ProductDetailDto>(
                Error.Failure(
                    code: "Product.GetFailed", 
                    message: $"Failed to retrieve product: {ex.Message}"));
        }
    }
}