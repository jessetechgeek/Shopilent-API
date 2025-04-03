using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Specifications;

namespace Shopilent.Domain.Payments.Specifications;

public class PaymentByMethodSpecification : Specification<Payment>
{
    private readonly Guid _paymentMethodId;

    public PaymentByMethodSpecification(Guid paymentMethodId)
    {
        _paymentMethodId = paymentMethodId;
    }

    public override bool IsSatisfiedBy(Payment payment)
    {
        return payment.PaymentMethodId == _paymentMethodId;
    }
}