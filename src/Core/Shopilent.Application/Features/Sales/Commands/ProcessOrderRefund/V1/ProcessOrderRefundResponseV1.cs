namespace Shopilent.Application.Features.Sales.Commands.ProcessOrderRefund.V1;

public sealed class ProcessOrderRefundResponseV1
{
    public Guid OrderId { get; init; }
    public decimal RefundAmount { get; init; }
    public string Currency { get; init; }
    public string Reason { get; init; }
    public DateTime RefundedAt { get; init; }
    public string Status { get; init; }
}