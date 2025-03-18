using Shopilent.Domain.Common;
using Shopilent.Domain.Sales.Enums;

namespace Shopilent.Domain.Sales.Specifications;

public class PendingOrderSpecification : Specification<Order>
{
    public override bool IsSatisfiedBy(Order order)
    {
        return order.Status == OrderStatus.Pending;
    }
}