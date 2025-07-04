using Shopilent.Domain.Sales.DTOs;

namespace Shopilent.API.Endpoints.Sales.GetRecentOrders.V1;

public class GetRecentOrdersResponseV1
{
    public IReadOnlyList<OrderDto> Orders { get; init; } = new List<OrderDto>();
    public int Count { get; init; }
    public DateTime RetrievedAt { get; init; }
}