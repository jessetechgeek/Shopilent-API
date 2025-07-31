using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Abstractions.Search;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Application.Features.Search.Commands.InitializeIndex.V1;

internal sealed class InitializeSearchIndexCommandHandlerV1 : ICommandHandler<InitializeSearchIndexCommandV1, InitializeSearchIndexResponseV1>
{
    private readonly ISearchService _searchService;
    private readonly ILogger<InitializeSearchIndexCommandHandlerV1> _logger;

    public InitializeSearchIndexCommandHandlerV1(
        ISearchService searchService,
        ILogger<InitializeSearchIndexCommandHandlerV1> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    public async Task<Result<InitializeSearchIndexResponseV1>> Handle(
        InitializeSearchIndexCommandV1 request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Initializing search indexes...");

            await _searchService.InitializeIndexesAsync(cancellationToken);

            var response = new InitializeSearchIndexResponseV1
            {
                IsSuccess = true,
                Message = "Search indexes initialized successfully",
                InitializedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Search indexes initialized successfully");
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize search indexes");
            
            var response = new InitializeSearchIndexResponseV1
            {
                IsSuccess = false,
                Message = $"Failed to initialize search indexes: {ex.Message}",
                InitializedAt = DateTime.UtcNow
            };

            return Result.Failure<InitializeSearchIndexResponseV1>(
                Domain.Common.Errors.Error.Failure("Search.InitializationFailed", response.Message));
        }
    }
}