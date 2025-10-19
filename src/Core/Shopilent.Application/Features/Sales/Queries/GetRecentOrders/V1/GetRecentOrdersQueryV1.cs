using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Sales.DTOs;

namespace Shopilent.Application.Features.Sales.Queries.GetRecentOrders.V1;

public sealed record GetRecentOrdersQueryV1 : IQuery<IReadOnlyList<OrderDto>>, ICachedQuery<IReadOnlyList<OrderDto>>
{
    public int Count { get; init; } = 10;

    public string CacheKey => $"recent-orders-{Count}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}