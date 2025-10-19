using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Sales.Enums;

namespace Shopilent.Application.Features.Sales.Commands.UpdateOrderStatus.V1;

public sealed record UpdateOrderStatusCommandV1 : ICommand<UpdateOrderStatusResponseV1>
{
    public Guid Id { get; init; }
    public OrderStatus Status { get; init; }
    public string Reason { get; init; }
}