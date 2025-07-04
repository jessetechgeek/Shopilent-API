using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Sales.Commands.CancelOrder.V1;

public sealed record CancelOrderCommandV1 : ICommand<CancelOrderResponseV1>
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; }
    public Guid? CurrentUserId { get; init; }
    public bool IsAdmin { get; init; }
    public bool IsManager { get; init; }
}