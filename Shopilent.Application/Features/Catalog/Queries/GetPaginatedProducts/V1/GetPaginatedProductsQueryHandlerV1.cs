using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Models;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Application.Features.Catalog.Queries.GetPaginatedProducts.V1;

internal sealed class GetPaginatedProductsQueryHandlerV1 : 
    IQueryHandler<GetPaginatedProductsQueryV1, PaginatedResult<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPaginatedProductsQueryHandlerV1> _logger;

    public GetPaginatedProductsQueryHandlerV1(
        IUnitOfWork unitOfWork,
        ILogger<GetPaginatedProductsQueryHandlerV1> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PaginatedResult<ProductDto>>> Handle(
        GetPaginatedProductsQueryV1 request, 
        CancellationToken cancellationToken)
    {
        try
        {
            PaginatedResult<ProductDto> paginatedResult;

            // If filtering by category, use the category-specific method
            if (request.CategoryId.HasValue)
            {
                paginatedResult = await _unitOfWork.ProductReader.GetPaginatedByCategoryAsync(
                    request.CategoryId.Value,
                    request.PageNumber,
                    request.PageSize,
                    request.SortColumn,
                    request.SortDescending,
                    cancellationToken);
            }
            else
            {
                // Use the general paginated method
                paginatedResult = await _unitOfWork.ProductReader.GetPaginatedAsync(
                    request.PageNumber,
                    request.PageSize,
                    request.SortColumn,
                    request.SortDescending,
                    cancellationToken);
            }

            // Filter active products if requested (this should ideally be done at the repository level)
            if (request.IsActiveOnly)
            {
                var activeProducts = paginatedResult.Items.Where(p => p.IsActive).ToList();
                paginatedResult = PaginatedResult<ProductDto>.Create(
                    activeProducts, 
                    activeProducts.Count, 
                    request.PageNumber, 
                    request.PageSize);
            }
            
            _logger.LogInformation(
                "Retrieved paginated products: Page {PageNumber}, Size {PageSize}, Total {TotalCount}, CategoryId {CategoryId}", 
                paginatedResult.PageNumber, 
                paginatedResult.PageSize, 
                paginatedResult.TotalCount,
                request.CategoryId);
                
            return Result.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paginated products");
            
            return Result.Failure<PaginatedResult<ProductDto>>(
                Error.Failure(
                    code: "Products.GetPaginatedFailed",
                    message: $"Failed to retrieve paginated products: {ex.Message}"));
        }
    }
}