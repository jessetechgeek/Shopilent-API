using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Models;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Application.Features.Catalog.Queries.GetProductsDatatable.V1;

internal sealed class GetProductsDatatableQueryHandlerV1 :
    IQueryHandler<GetProductsDatatableQueryV1, DataTableResult<ProductDatatableDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetProductsDatatableQueryHandlerV1> _logger;

    public GetProductsDatatableQueryHandlerV1(
        IUnitOfWork unitOfWork,
        ILogger<GetProductsDatatableQueryHandlerV1> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<DataTableResult<ProductDatatableDto>>> Handle(
        GetProductsDatatableQueryV1 request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get detailed datatable results from repository
            var result = await _unitOfWork.ProductReader.GetProductDetailDataTableAsync(
                request.Request,
                cancellationToken);

            // Map the product details to the DTO
            var dtoItems = result.Data.Select(productDetail => new ProductDatatableDto
            {
                Id = productDetail.Id,
                Name = productDetail.Name,
                Slug = productDetail.Slug,
                Description = productDetail.Description,
                BasePrice = productDetail.BasePrice,
                Currency = productDetail.Currency,
                Sku = productDetail.Sku,
                IsActive = productDetail.IsActive,
                VariantsCount = productDetail.Variants?.Count ?? 0,
                TotalStockQuantity = productDetail.Variants?.Sum(v => v.StockQuantity) ?? 0,
                Categories = productDetail.Categories?.Select(c => c.Name).ToList() ?? new List<string>(),
                CreatedAt = productDetail.CreatedAt,
                UpdatedAt = productDetail.UpdatedAt
            }).ToList();

            // Create new datatable result with mapped DTOs
            var datatableResult = new DataTableResult<ProductDatatableDto>(
                result.Draw,
                result.RecordsTotal,
                result.RecordsFiltered,
                dtoItems);

            _logger.LogInformation("Retrieved {Count} products for datatable", dtoItems.Count);
            return Result.Success(datatableResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products for datatable");

            return Result.Failure<DataTableResult<ProductDatatableDto>>(
                Error.Failure(
                    code: "Products.GetDataTableFailed",
                    message: $"Failed to retrieve products: {ex.Message}"));
        }
    }
}