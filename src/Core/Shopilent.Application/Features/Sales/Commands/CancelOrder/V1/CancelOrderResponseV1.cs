using Shopilent.Domain.Sales.Enums;

namespace Shopilent.Application.Features.Sales.Commands.CancelOrder.V1;

public class CancelOrderResponseV1
{
    public Guid OrderId { get; init; }
    public OrderStatus Status { get; init; }
    public string Reason { get; init; }
    public DateTime CancelledAt { get; init; }
}