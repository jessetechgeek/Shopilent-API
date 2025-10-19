using Shopilent.Domain.Sales.Enums;

namespace Shopilent.API.Endpoints.Sales.UpdateOrderStatus.V1;

public class UpdateOrderStatusRequestV1
{
    public OrderStatus Status { get; init; }
    public string Reason { get; init; }
}