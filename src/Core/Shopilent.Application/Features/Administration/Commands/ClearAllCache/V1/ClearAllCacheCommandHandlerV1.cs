using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Application.Features.Administration.Commands.ClearAllCache.V1;

internal sealed class ClearAllCacheCommandHandlerV1 : ICommandHandler<ClearAllCacheCommandV1, ClearAllCacheResponseV1>
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<ClearAllCacheCommandHandlerV1> _logger;

    public ClearAllCacheCommandHandlerV1(
        ICacheService cacheService,
        ILogger<ClearAllCacheCommandHandlerV1> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<ClearAllCacheResponseV1>> Handle(
        ClearAllCacheCommandV1 request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting cache clear operation");

            var keysCleared = await _cacheService.ClearAllAsync(cancellationToken);

            var response = new ClearAllCacheResponseV1
            {
                Message = $"Successfully cleared all cache. {keysCleared} keys removed.",
                ClearedAt = DateTime.UtcNow,
                KeysCleared = keysCleared
            };

            _logger.LogInformation("Cache clear operation completed successfully. Keys cleared: {KeysCleared}", keysCleared);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while clearing cache");
            
            return Result.Failure<ClearAllCacheResponseV1>(
                Error.Failure(
                    code: "ClearAllCache.Failed",
                    message: $"Failed to clear cache: {ex.Message}"));
        }
    }
}