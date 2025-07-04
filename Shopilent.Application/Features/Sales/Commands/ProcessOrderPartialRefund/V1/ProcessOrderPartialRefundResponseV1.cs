namespace Shopilent.Application.Features.Sales.Commands.ProcessOrderPartialRefund.V1;

public sealed class ProcessOrderPartialRefundResponseV1
{
    public Guid OrderId { get; set; }
    public decimal RefundAmount { get; set; }
    public string Currency { get; set; }
    public decimal TotalRefundedAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string Reason { get; set; }
    public DateTime RefundedAt { get; set; }
    public bool IsFullyRefunded { get; set; }
}