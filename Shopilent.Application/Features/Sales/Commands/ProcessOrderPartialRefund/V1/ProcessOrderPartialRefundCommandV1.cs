using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Sales.Commands.ProcessOrderPartialRefund.V1;

public sealed record ProcessOrderPartialRefundCommandV1 : ICommand<ProcessOrderPartialRefundResponseV1>
{
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public string Reason { get; init; }
}