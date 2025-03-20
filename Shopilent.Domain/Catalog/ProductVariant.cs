using Shopilent.Domain.Common;
using Shopilent.Domain.Sales.ValueObjects;

namespace Shopilent.Domain.Catalog;

public class ProductVariant : Entity
{
    private ProductVariant()
    {
        // Required by EF Core
    }

    private ProductVariant(Product product, string sku = null, Money price = null, int stockQuantity = 0)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        ProductId = product.Id;
        Sku = sku;
        Price = price;
        StockQuantity = stockQuantity;
        IsActive = true;
        Metadata = new Dictionary<string, object>();

        _variantAttributes = new List<VariantAttribute>();
    }

    public static ProductVariant Create(Product product, string sku = null, Money price = null, int stockQuantity = 0)
    {
        return new ProductVariant(product, sku, price, stockQuantity);
    }

    public static ProductVariant CreateInactive(Product product, string sku = null, Money price = null,
        int stockQuantity = 0)
    {
        var variant = new ProductVariant(product, sku, price, stockQuantity);
        variant.IsActive = false;
        return variant;
    }

    public static ProductVariant CreateOutOfStock(Product product, string sku = null, Money price = null)
    {
        return new ProductVariant(product, sku, price, 0);
    }

    public Guid ProductId { get; private set; }
    public string Sku { get; private set; }
    public Money Price { get; private set; }
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; }
    public Dictionary<string, object> Metadata { get; private set; } = new();

    private readonly List<VariantAttribute> _variantAttributes = new();
    public IReadOnlyCollection<VariantAttribute> Attributes => _variantAttributes.AsReadOnly();

    public void Update(string sku, Money price)
    {
        Sku = sku;
        Price = price;
    }

    public void SetStockQuantity(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative", nameof(quantity));

        StockQuantity = quantity;
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        StockQuantity += quantity;
    }

    public bool RemoveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (StockQuantity < quantity)
            return false;

        StockQuantity -= quantity;
        return true;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void AddAttribute(Attribute attribute, object value)
    {
        if (attribute == null)
            throw new ArgumentNullException(nameof(attribute));

        if (!attribute.IsVariant)
            throw new InvalidOperationException("Only variant attributes can be added to a variant.");

        if (_variantAttributes.Exists(va => va.AttributeId == attribute.Id))
            return;

        var variantAttribute = VariantAttribute.Create(this, attribute, value);
        _variantAttributes.Add(variantAttribute);
    }

    public void UpdateMetadata(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be empty", nameof(key));

        Metadata[key] = value;
    }
}