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

    // Static factory method for internal use by Cart aggregate
    internal static CartItem Create(Cart cart, Product product, int quantity = 1, ProductVariant variant = null)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (!product.IsActive)
            throw new ArgumentException("Product is not active", nameof(product));

        if (variant != null && !variant.IsActive)
            throw new ArgumentException("Variant is not active", nameof(variant));

        return new CartItem(cart, product, quantity, variant);
    }

    // For use by the Cart aggregate which should validate inputs
    internal static Result<CartItem> Create(Result<Cart> cartResult, Product product, int quantity = 1,
        ProductVariant variant = null)
    {
        if (cartResult.IsFailure)
            return Result.Failure<CartItem>(cartResult.Error);

        if (product == null)
            return Result.Failure<CartItem>(ProductErrors.NotFound(Guid.Empty));

        if (quantity <= 0)
            return Result.Failure<CartItem>(CartErrors.InvalidQuantity);

        if (!product.IsActive)
            return Result.Failure<CartItem>(CartErrors.ProductUnavailable(product.Id));

        if (variant != null && !variant.IsActive)
            return Result.Failure<CartItem>(CartErrors.ProductVariantNotAvailable(variant.Id));

        return Result.Success(new CartItem(cartResult.Value, product, quantity, variant));
    }

    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? VariantId { get; private set; }
    public int Quantity { get; private set; }

    // Internal method for Cart aggregate to update quantity
    internal Result UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            return Result.Failure(CartErrors.InvalidQuantity);

        Quantity = quantity;
        return Result.Success();
    }
}