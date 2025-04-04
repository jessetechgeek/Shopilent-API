using Shopilent.Domain.Common.Repositories.Read;
using Shopilent.Domain.Sales.DTOs;
using Shopilent.Domain.Sales.Enums;

namespace Shopilent.Domain.Sales.Repositories.Read;

public interface IOrderReadRepository : IAggregateReadRepository<OrderDto>
{
    Task<OrderDetailDto> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderDto>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderDto>> GetRecentOrdersAsync(int count, CancellationToken cancellationToken = default);
    Task<OrderItemDto> GetOrderItemByIdAsync(Guid orderItemId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderDto>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}