using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Sales.Commands.ProcessOrderRefund.V1;

public sealed record ProcessOrderRefundCommandV1 : ICommand<ProcessOrderRefundResponseV1>
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; }
}