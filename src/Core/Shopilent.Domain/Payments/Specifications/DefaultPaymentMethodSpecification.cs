using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Specifications;

namespace Shopilent.Domain.Payments.Specifications;

public class DefaultPaymentMethodSpecification : Specification<PaymentMethod>
{
    public override bool IsSatisfiedBy(PaymentMethod paymentMethod)
    {
        return paymentMethod.IsDefault;
    }
}