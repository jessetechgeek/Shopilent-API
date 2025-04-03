using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Specifications;

namespace Shopilent.Domain.Sales.Specifications;

public class OrderWithProductSpecification : Specification<Order>
{
    private readonly Guid _productId;

    public OrderWithProductSpecification(Guid productId)
    {
        _productId = productId;
    }

    public override bool IsSatisfiedBy(Order order)
    {
        return order.Items.Any(i => i.ProductId == _productId);
    }
}