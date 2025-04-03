using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Specifications;
using Shopilent.Domain.Sales.Enums;

namespace Shopilent.Domain.Sales.Specifications;

public class CompletedOrderSpecification : Specification<Order>
{
    public override bool IsSatisfiedBy(Order order)
    {
        return order.Status == OrderStatus.Delivered;
    }
}