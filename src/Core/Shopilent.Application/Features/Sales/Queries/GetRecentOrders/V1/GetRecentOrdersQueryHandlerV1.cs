using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Sales.DTOs;

namespace Shopilent.Application.Features.Sales.Queries.GetRecentOrders.V1;

internal sealed class GetRecentOrdersQueryHandlerV1 : IQueryHandler<GetRecentOrdersQueryV1, IReadOnlyList<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetRecentOrdersQueryHandlerV1> _logger;

    public GetRecentOrdersQueryHandlerV1(
        IUnitOfWork unitOfWork,
        ILogger<GetRecentOrdersQueryHandlerV1> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<OrderDto>>> Handle(
        GetRecentOrdersQueryV1 request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var orders = await _unitOfWork.OrderReader.GetRecentOrdersAsync(request.Count, cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} recent orders for dashboard", orders.Count);
            return Result.Success(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent orders for dashboard");
            
            return Result.Failure<IReadOnlyList<OrderDto>>(
                Error.Failure(
                    code: "Orders.GetRecentFailed",
                    message: $"Failed to retrieve recent orders: {ex.Message}"));
        }
    }
}