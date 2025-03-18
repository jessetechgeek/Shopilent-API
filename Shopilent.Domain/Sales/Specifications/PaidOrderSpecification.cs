using Shopilent.Domain.Common;
using Shopilent.Domain.Payments.Enums;

namespace Shopilent.Domain.Sales.Specifications;

public class PaidOrderSpecification : Specification<Order>
{
    public override bool IsSatisfiedBy(Order order)
    {
        return order.PaymentStatus == PaymentStatus.Succeeded;
    }
}