using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Application.Features.Catalog.Queries.GetVariant.V1;

internal sealed class GetVariantQueryHandlerV1 : IQueryHandler<GetVariantQueryV1, ProductVariantDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetVariantQueryHandlerV1> _logger;

    public GetVariantQueryHandlerV1(
        IUnitOfWork unitOfWork,
        ILogger<GetVariantQueryHandlerV1> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ProductVariantDto>> Handle(
        GetVariantQueryV1 request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var variant = await _unitOfWork.ProductVariantReader.GetByIdAsync(request.Id, cancellationToken);
            
            if (variant == null)
            {
                _logger.LogWarning("Product variant with ID {VariantId} was not found", request.Id);
                return Result.Failure<ProductVariantDto>(
                    Error.NotFound(message: $"Product variant with ID {request.Id} not found"));
            }

            _logger.LogInformation("Retrieved product variant with ID {VariantId}", request.Id);
            return Result.Success(variant);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product variant with ID {VariantId}", request.Id);
            
            return Result.Failure<ProductVariantDto>(
                Error.Failure(
                    code: "ProductVariant.GetFailed", 
                    message: $"Failed to retrieve product variant: {ex.Message}"));
        }
    }
}