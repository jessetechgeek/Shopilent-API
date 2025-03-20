using Shopilent.Domain.Catalog.Events;
using Shopilent.Domain.Catalog.ValueObjects;
using Shopilent.Domain.Common;
using Shopilent.Domain.Sales.ValueObjects;

namespace Shopilent.Domain.Catalog;

public class Product : AggregateRoot
{
    private Product()
    {
        // Required by EF Core
    }

    private Product(string name, Slug slug, Money basePrice, string sku = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(slug.Value))
            throw new ArgumentException("Slug cannot be empty", nameof(slug));

        if (basePrice == null)
            throw new ArgumentNullException(nameof(basePrice));

        Name = name;
        Slug = slug;
        BasePrice = basePrice;
        Sku = sku;
        IsActive = true;
        Metadata = new Dictionary<string, object>();

        _productCategories = new List<ProductCategory>();
        _attributes = new List<ProductAttribute>();
        _variants = new List<ProductVariant>();
    }

    public static Product Create(string name, Slug slug, Money basePrice, string sku = null)
    {
        var product = new Product(name, slug, basePrice, sku);
        product.AddDomainEvent(new ProductCreatedEvent(product.Id));
        return product;
    }

    public static Product CreateWithDescription(string name, Slug slug, Money basePrice, string description,
        string sku = null)
    {
        var product = Create(name, slug, basePrice, sku);
        product.Description = description;
        return product;
    }

    public static Product CreateInactive(string name, Slug slug, Money basePrice, string sku = null)
    {
        var product = new Product(name, slug, basePrice, sku);
        product.IsActive = false;
        product.AddDomainEvent(new ProductCreatedEvent(product.Id));
        return product;
    }

    public string Name { get; private set; }
    public string Description { get; private set; }
    public Money BasePrice { get; private set; }
    public string Sku { get; private set; }
    public Slug Slug { get; private set; }
    public Dictionary<string, object> Metadata { get; private set; } = new Dictionary<string, object>();
    public bool IsActive { get; private set; }

    private readonly List<ProductCategory> _productCategories = new();
    public IReadOnlyCollection<ProductCategory> Categories => _productCategories.AsReadOnly();

    private readonly List<ProductAttribute> _attributes = new();
    public IReadOnlyCollection<ProductAttribute> Attributes => _attributes.AsReadOnly();

    private readonly List<ProductVariant> _variants = new();
    public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();

    public void Update(string name, Slug slug, Money basePrice, string description = null, string sku = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(slug.Value))
            throw new ArgumentException("Slug cannot be empty", nameof(slug));

        if (basePrice == null)
            throw new ArgumentNullException(nameof(basePrice));

        Name = name;
        Slug = slug;
        BasePrice = basePrice;
        Description = description;
        Sku = sku;

        AddDomainEvent(new ProductUpdatedEvent(Id));
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        AddDomainEvent(new ProductStatusChangedEvent(Id, true));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        AddDomainEvent(new ProductStatusChangedEvent(Id, false));
    }

    public void AddCategory(Category category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));

        if (_productCategories.Exists(pc => pc.CategoryId == category.Id))
            return;

        var productCategory = ProductCategory.Create(this, category);
        _productCategories.Add(productCategory);
        AddDomainEvent(new ProductCategoryAddedEvent(Id, category.Id));
    }

    public void RemoveCategory(Category category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));

        var productCategory = _productCategories.Find(pc => pc.CategoryId == category.Id);
        if (productCategory != null)
        {
            _productCategories.Remove(productCategory);
            AddDomainEvent(new ProductCategoryRemovedEvent(Id, category.Id));
        }
    }

    public void AddAttribute(Attribute attribute, object value)
    {
        if (attribute == null)
            throw new ArgumentNullException(nameof(attribute));

        if (_attributes.Exists(pa => pa.AttributeId == attribute.Id))
            return;

        var productAttribute = ProductAttribute.Create(this, attribute, value);
        _attributes.Add(productAttribute);
    }

    public void AddVariant(ProductVariant variant)
    {
        if (variant == null)
            throw new ArgumentNullException(nameof(variant));

        if (_variants.Exists(v => v.Sku == variant.Sku && !string.IsNullOrEmpty(variant.Sku)))
            throw new InvalidOperationException($"A variant with SKU '{variant.Sku}' already exists.");

        _variants.Add(variant);
        AddDomainEvent(new ProductVariantAddedEvent(Id, variant.Id));
    }

    public void UpdateMetadata(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be empty", nameof(key));

        Metadata[key] = value;
    }
}