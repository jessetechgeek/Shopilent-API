using Shopilent.Domain.Common;

namespace Shopilent.Domain.Sales.Specifications;

public class OrderByUserSpecification : Specification<Order>
{
    private readonly Guid _userId;

    public OrderByUserSpecification(Guid userId)
    {
        _userId = userId;
    }

    public override bool IsSatisfiedBy(Order order)
    {
        return order.UserId == _userId;
    }
}