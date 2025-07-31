using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Application.Abstractions.Search;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Application.Features.Search.Commands.ReindexProducts.V1;

internal sealed class
    ReindexProductsCommandHandlerV1 : ICommandHandler<ReindexProductsCommandV1, ReindexProductsResponseV1>
{
    private readonly ISearchService _searchService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReindexProductsCommandHandlerV1> _logger;

    public ReindexProductsCommandHandlerV1(
        ISearchService searchService,
        IUnitOfWork unitOfWork,
        ILogger<ReindexProductsCommandHandlerV1> logger)
    {
        _searchService = searchService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ReindexProductsResponseV1>> Handle(
        ReindexProductsCommandV1 request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Starting product reindexing...");

            var productDtos = await _unitOfWork.ProductReader.ListAllAsync(cancellationToken);
            var productDtoList = productDtos.ToList();

            if (!productDtoList.Any())
            {
                var emptyResponse = new ReindexProductsResponseV1
                {
                    IsSuccess = true,
                    Message = "No products found to index",
                    ProductsIndexed = 0,
                    IndexedAt = DateTime.UtcNow,
                    Duration = stopwatch.Elapsed
                };

                return Result.Success(emptyResponse);
            }

            var searchDocuments = new List<ProductSearchDocument>();
            foreach (var productDto in productDtoList)
            {
                var product = await _unitOfWork.ProductReader.GetDetailByIdAsync(productDto.Id, cancellationToken);
                if (product != null)
                {
                    searchDocuments.Add(ProductSearchDocument.FromProductDto(product));
                }
            }

            var indexResult = await _searchService.IndexProductsAsync(searchDocuments, cancellationToken);

            stopwatch.Stop();

            if (indexResult.IsFailure)
            {
                _logger.LogError("Failed to reindex products: {Error}", indexResult.Error.Message);

                var failureResponse = new ReindexProductsResponseV1
                {
                    IsSuccess = false,
                    Message = $"Failed to reindex products: {indexResult.Error.Message}",
                    ProductsIndexed = 0,
                    IndexedAt = DateTime.UtcNow,
                    Duration = stopwatch.Elapsed
                };

                return Result.Failure<ReindexProductsResponseV1>(indexResult.Error);
            }

            var response = new ReindexProductsResponseV1
            {
                IsSuccess = true,
                Message = $"Successfully reindexed {searchDocuments.Count} products",
                ProductsIndexed = searchDocuments.Count,
                IndexedAt = DateTime.UtcNow,
                Duration = stopwatch.Elapsed
            };

            _logger.LogInformation("Successfully reindexed {Count} products in {Duration}ms",
                searchDocuments.Count, stopwatch.ElapsedMilliseconds);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Unexpected error during product reindexing");

            var response = new ReindexProductsResponseV1
            {
                IsSuccess = false,
                Message = $"Unexpected error during reindexing: {ex.Message}",
                ProductsIndexed = 0,
                IndexedAt = DateTime.UtcNow,
                Duration = stopwatch.Elapsed
            };

            return Result.Failure<ReindexProductsResponseV1>(
                Domain.Common.Errors.Error.Failure("Search.ReindexFailed", response.Message));
        }
    }
}