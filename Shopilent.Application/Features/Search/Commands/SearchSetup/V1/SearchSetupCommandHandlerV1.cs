using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Application.Abstractions.Search;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Application.Features.Search.Commands.SearchSetup.V1;

internal sealed class SearchSetupCommandHandlerV1 : ICommandHandler<SearchSetupCommandV1, SearchSetupResponseV1>
{
    private readonly ISearchService _searchService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SearchSetupCommandHandlerV1> _logger;

    public SearchSetupCommandHandlerV1(
        ISearchService searchService,
        IUnitOfWork unitOfWork,
        ILogger<SearchSetupCommandHandlerV1> logger)
    {
        _searchService = searchService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<SearchSetupResponseV1>> Handle(
        SearchSetupCommandV1 request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = new SearchSetupResponseV1 { CompletedAt = DateTime.UtcNow };

        try
        {
            _logger.LogInformation("Starting search setup - Initialize: {Initialize}, Index: {Index}, Force: {Force}",
                request.InitializeIndexes, request.IndexProducts, request.ForceReindex);

            if (request.InitializeIndexes)
            {
                _logger.LogInformation("Initializing search indexes...");
                await _searchService.InitializeIndexesAsync(cancellationToken);
                response.IndexesInitialized = true;
                _logger.LogInformation("Search indexes initialized successfully");
            }

            if (request.IndexProducts)
            {
                _logger.LogInformation("Starting product indexing...");

                var productDtos = await _unitOfWork.ProductReader.ListAllAsync(cancellationToken);
                var productDtoList = productDtos.ToList();

                if (!productDtoList.Any())
                {
                    _logger.LogInformation("No products found to index");
                    response.ProductsIndexed = 0;
                }
                else
                {
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

                    if (indexResult.IsFailure)
                    {
                        _logger.LogError("Failed to index products: {Error}", indexResult.Error.Message);
                        stopwatch.Stop();

                        response.IsSuccess = false;
                        response.Message = $"Search setup partially completed. Index initialization: {(response.IndexesInitialized ? "Success" : "Skipped")}. Product indexing failed: {indexResult.Error.Message}";
                        response.Duration = stopwatch.Elapsed;

                        return Result.Failure<SearchSetupResponseV1>(indexResult.Error);
                    }

                    response.ProductsIndexed = searchDocuments.Count;
                    _logger.LogInformation("Successfully indexed {Count} products", searchDocuments.Count);
                }
            }

            stopwatch.Stop();

            var messageParts = new List<string>();
            if (request.InitializeIndexes)
                messageParts.Add("indexes initialized");
            if (request.IndexProducts)
                messageParts.Add($"{response.ProductsIndexed} products indexed");

            response.IsSuccess = true;
            response.Message = $"Search setup completed successfully: {string.Join(", ", messageParts)}";
            response.Duration = stopwatch.Elapsed;

            _logger.LogInformation("Search setup completed successfully in {Duration}ms - Indexes: {Indexes}, Products: {Products}",
                stopwatch.ElapsedMilliseconds, response.IndexesInitialized, response.ProductsIndexed);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Unexpected error during search setup");

            response.IsSuccess = false;
            response.Message = $"Search setup failed: {ex.Message}";
            response.Duration = stopwatch.Elapsed;

            return Result.Failure<SearchSetupResponseV1>(
                Domain.Common.Errors.Error.Failure("Search.SetupFailed", response.Message));
        }
    }
}