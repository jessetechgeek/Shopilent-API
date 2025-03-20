using Shopilent.Domain.Catalog;
using Shopilent.Domain.Common;

namespace Shopilent.Domain.Sales;

public class CartItem : Entity
{
    private CartItem()
    {
        // Required by EF Core
    }

    private CartItem(Cart cart, Product product, int quantity = 1, ProductVariant variant = null)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        CartId = cart.Id;
        ProductId = product.Id;
        VariantId = variant?.Id;
        Quantity = quantity;
    }

    // Add static factory method
    public static CartItem Create(Cart cart, Product product, int quantity = 1, ProductVariant variant = null)
    {
        return new CartItem(cart, product, quantity, variant);
    }

    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? VariantId { get; private set; }
    public int Quantity { get; private set; }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        Quantity = quantity;
    }
}