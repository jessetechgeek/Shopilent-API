using Shopilent.Domain.Common;
using Shopilent.Domain.Payments.Enums;

namespace Shopilent.Domain.Payments.Specifications;

public class PaymentByProviderSpecification : Specification<Payment>
{
    private readonly PaymentProvider _provider;

    public PaymentByProviderSpecification(PaymentProvider provider)
    {
        _provider = provider;
    }

    public override bool IsSatisfiedBy(Payment payment)
    {
        return payment.Provider == _provider;
    }
}