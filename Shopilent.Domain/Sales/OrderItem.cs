using Shopilent.Domain.Catalog;
using Shopilent.Domain.Common;
using Shopilent.Domain.Sales.ValueObjects;

namespace Shopilent.Domain.Sales;

public class OrderItem : Entity
{
    private OrderItem()
    {
        // Required by EF Core
    }

    private OrderItem(Order order, Product product, int quantity, Money unitPrice, ProductVariant variant = null)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (unitPrice == null)
            throw new ArgumentNullException(nameof(unitPrice));

        OrderId = order.Id;
        ProductId = product.Id;
        VariantId = variant?.Id;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalPrice = unitPrice.Multiply(quantity);

        // Create snapshot of product data
        ProductData = new Dictionary<string, object>
        {
            { "name", product.Name },
            { "sku", product.Sku },
            { "slug", product.Slug }
        };

        if (variant != null)
        {
            ProductData["variant_sku"] = variant.Sku;
            ProductData["variant_attributes"] = variant.Attributes;
        }
    }

    // Add static factory method
    public static OrderItem Create(Order order, Product product, int quantity, Money unitPrice, ProductVariant variant = null)
    {
        return new OrderItem(order, product, quantity, unitPrice, variant);
    }

    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? VariantId { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public Money TotalPrice { get; private set; }
    public Dictionary<string, object> ProductData { get; private set; } = new();
}