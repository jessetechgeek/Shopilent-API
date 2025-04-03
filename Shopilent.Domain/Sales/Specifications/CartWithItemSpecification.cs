using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Specifications;

namespace Shopilent.Domain.Sales.Specifications;

public class CartWithItemSpecification : Specification<Cart>
{
    private readonly Guid _productId;
    private readonly Guid? _variantId;

    public CartWithItemSpecification(Guid productId, Guid? variantId = null)
    {
        _productId = productId;
        _variantId = variantId;
    }

    public override bool IsSatisfiedBy(Cart cart)
    {
        return cart.Items.Any(i => 
            i.ProductId == _productId && 
            (_variantId == null || i.VariantId == _variantId));
    }
}