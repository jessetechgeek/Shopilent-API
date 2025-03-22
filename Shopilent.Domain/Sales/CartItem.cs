using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.Errors;
using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Sales.Errors;

namespace Shopilent.Domain.Sales;

public class CartItem : Entity
{
    private CartItem()
    {
        // Required by EF Core
    }

    private CartItem(Cart cart, Product product, int quantity = 1, ProductVariant variant = null)
    {
        CartId = cart.Id;
        ProductId = product.Id;
        VariantId = variant?.Id;
        Quantity = quantity;
    }

    // Add static factory method
    public static Result<CartItem> Create(Cart cart, Product product, int quantity = 1, ProductVariant variant = null)
    {
        if (cart == null)
            return Result.Failure<CartItem>(CartErrors.CartNotFound(Guid.Empty));
            
        if (product == null)
            return Result.Failure<CartItem>(ProductErrors.NotFound(Guid.Empty));
            
        if (quantity <= 0)
            return Result.Failure<CartItem>(CartErrors.InvalidQuantity);
            
        if (!product.IsActive)
            return Result.Failure<CartItem>(ProductErrors.InactiveProduct);
            
        if (variant != null && !variant.IsActive)
            return Result.Failure<CartItem>(ProductVariantErrors.InactiveVariant);

        return Result.Success(new CartItem(cart, product, quantity, variant));
    }

    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? VariantId { get; private set; }
    public int Quantity { get; private set; }

    public Result UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            return Result.Failure(CartErrors.InvalidQuantity);

        Quantity = quantity;
        return Result.Success();
    }
}