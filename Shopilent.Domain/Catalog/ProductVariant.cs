using Shopilent.Domain.Catalog.Errors;
using Shopilent.Domain.Catalog.Events;
using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Results;
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
        ProductId = product.Id;
        Sku = sku;
        Price = price;
        StockQuantity = stockQuantity;
        IsActive = true;
        Metadata = new Dictionary<string, object>();

        _variantAttributes = new List<VariantAttribute>();
    }

    public static Result<ProductVariant> Create(Product product, string sku = null, Money price = null,
        int stockQuantity = 0)
    {
        if (product == null)
            return Result.Failure<ProductVariant>(ProductErrors.NotFound(Guid.Empty));

        if (stockQuantity < 0)
            return Result.Failure<ProductVariant>(ProductVariantErrors.NegativeStockQuantity);

        if (price != null && price.Amount < 0)
            return Result.Failure<ProductVariant>(ProductVariantErrors.NegativePrice);

        var variant = new ProductVariant(product, sku, price, stockQuantity);
        return Result.Success(variant);
    }

    public static Result<ProductVariant> CreateInactive(Product product, string sku = null, Money price = null,
        int stockQuantity = 0)
    {
        var result = Create(product, sku, price, stockQuantity);
        if (result.IsFailure)
            return result;

        var variant = result.Value;
        variant.IsActive = false;
        return Result.Success(variant);
    }

    public static Result<ProductVariant> CreateOutOfStock(Product product, string sku = null, Money price = null)
    {
        return Create(product, sku, price, 0);
    }

    public Guid ProductId { get; private set; }
    public string Sku { get; private set; }
    public Money Price { get; private set; }
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; }
    public Dictionary<string, object> Metadata { get; private set; } = new();

    private readonly List<VariantAttribute> _variantAttributes = new();
    public IReadOnlyCollection<VariantAttribute> Attributes => _variantAttributes.AsReadOnly();

    public Result Update(string sku, Money price, Product product = null)
    {
        if (price != null && price.Amount < 0)
            return Result.Failure(ProductVariantErrors.NegativePrice);

        Sku = sku;
        Price = price;

        if (product != null)
            product.RaiseVariantEvent(new ProductVariantUpdatedEvent(ProductId, Id));

        return Result.Success();
    }

    public Result SetStockQuantity(int quantity, Product product = null)
    {
        if (quantity < 0)
            return Result.Failure(ProductVariantErrors.NegativeStockQuantity);

        var oldQuantity = StockQuantity;
        StockQuantity = quantity;

        if (product != null)
            product.RaiseVariantEvent(new ProductVariantStockChangedEvent(ProductId, Id, oldQuantity, quantity));

        return Result.Success();
    }

    public Result AddStock(int quantity, Product product = null)
    {
        if (quantity <= 0)
            return Result.Failure(ProductVariantErrors.NegativeStockQuantity);

        var oldQuantity = StockQuantity;
        StockQuantity += quantity;

        if (product != null)
            product.RaiseVariantEvent(new ProductVariantStockChangedEvent(ProductId, Id, oldQuantity, StockQuantity));

        return Result.Success();
    }

    public Result RemoveStock(int quantity, Product product = null)
    {
        if (quantity <= 0)
            return Result.Failure(ProductVariantErrors.NegativeStockQuantity);

        if (StockQuantity < quantity)
            return Result.Failure(ProductVariantErrors.InsufficientStock(quantity, StockQuantity));

        var oldQuantity = StockQuantity;
        StockQuantity -= quantity;

        if (product != null)
            product.RaiseVariantEvent(new ProductVariantStockChangedEvent(ProductId, Id, oldQuantity, StockQuantity));

        return Result.Success();
    }

    public Result Activate(Product product = null)
    {
        if (IsActive)
            return Result.Success();

        IsActive = true;

        if (product != null)
            product.RaiseVariantEvent(new ProductVariantUpdatedEvent(ProductId, Id));

        return Result.Success();
    }

    public Result Deactivate(Product product = null)
    {
        if (!IsActive)
            return Result.Success();

        IsActive = false;

        if (product != null)
            product.RaiseVariantEvent(new ProductVariantUpdatedEvent(ProductId, Id));

        return Result.Success();
    }

    public Result AddAttribute(Attribute attribute, object value, Product product = null)
    {
        if (attribute == null)
            return Result.Failure(AttributeErrors.NotFound(Guid.Empty));

        if (!attribute.IsVariant)
            return Result.Failure(ProductVariantErrors.NonVariantAttribute(attribute.Name));

        if (_variantAttributes.Exists(va => va.AttributeId == attribute.Id))
            return Result.Success(); // Already exists

        var variantAttributeResult = VariantAttribute.Create(this, attribute, value);
        if (variantAttributeResult.IsFailure)
            return Result.Failure(variantAttributeResult.Error);

        _variantAttributes.Add(variantAttributeResult.Value);

        if (product != null)
            product.RaiseVariantEvent(new ProductVariantUpdatedEvent(ProductId, Id));

        return Result.Success();
    }

    public Result UpdateMetadata(string key, object value, Product product = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Result.Failure(ProductVariantErrors.InvalidMetadataKey);

        Metadata[key] = value;

        if (product != null)
            product.RaiseVariantEvent(new ProductVariantUpdatedEvent(ProductId, Id));

        return Result.Success();
    }

    // Helper method to check if a variant attribute with specified ID exists
    public bool HasAttribute(Guid attributeId)
    {
        return _variantAttributes.Exists(a => a.AttributeId == attributeId);
    }

    // Get an attribute value
    public Result<object> GetAttributeValue(Guid attributeId)
    {
        var attribute = _variantAttributes.Find(a => a.AttributeId == attributeId);
        if (attribute == null)
            return Result.Failure<object>(AttributeErrors.NotFound(attributeId));

        if (!attribute.Value.ContainsKey("value"))
            return Result.Failure<object>(AttributeErrors.InvalidConfigurationFormat);

        return Result.Success(attribute.Value["value"]);
    }

    // Update an attribute value
    public Result UpdateAttributeValue(Guid attributeId, object value, Product product = null)
    {
        var attribute = _variantAttributes.Find(a => a.AttributeId == attributeId);
        if (attribute == null)
            return Result.Failure(AttributeErrors.NotFound(attributeId));

        var updateResult = attribute.UpdateValue(value);
        if (updateResult.IsFailure)
            return updateResult;

        if (product != null)
            product.RaiseVariantEvent(new ProductVariantUpdatedEvent(ProductId, Id));

        return Result.Success();
    }
}