using Shopilent.Domain.Common;

namespace Shopilent.Domain.Payments.Specifications;

public class PaymentByUserSpecification : Specification<Payment>
{
    private readonly Guid _userId;

    public PaymentByUserSpecification(Guid userId)
    {
        _userId = userId;
    }

    public override bool IsSatisfiedBy(Payment payment)
    {
        return payment.UserId == _userId;
    }
}