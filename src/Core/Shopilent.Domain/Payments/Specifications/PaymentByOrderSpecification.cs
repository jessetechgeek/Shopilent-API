using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Specifications;

namespace Shopilent.Domain.Payments.Specifications;

public class PaymentByOrderSpecification : Specification<Payment>
{
    private readonly Guid _orderId;

    public PaymentByOrderSpecification(Guid orderId)
    {
        _orderId = orderId;
    }

    public override bool IsSatisfiedBy(Payment payment)
    {
        return payment.OrderId == _orderId;
    }
}