namespace Shopilent.API.Endpoints.Sales.ProcessOrderPartialRefund.V1;

public sealed class ProcessOrderPartialRefundRequestV1
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Reason { get; set; }
}