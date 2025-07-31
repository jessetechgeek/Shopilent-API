using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Catalog.Enums;

namespace Shopilent.Application.Abstractions.Search;

public partial class ProductSearchDocument
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string SKU { get; init; } = "";
    public string Slug { get; init; } = "";
    public decimal BasePrice { get; init; }
    
    public ProductSearchCategory[] Categories { get; init; } = [];
    public ProductSearchAttribute[] Attributes { get; init; } = [];
    public ProductSearchVariant[] Variants { get; init; } = [];
    public ProductSearchImage[] Images { get; init; } = [];
    
    public string Status { get; init; } = "";
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    
    public ProductSearchPriceRange PriceRange { get; init; } = new();
    public Dictionary<string, string[]> AttributeFilters { get; init; } = new();
    public Guid[] CategoryIds { get; init; } = [];
    public string[] VariantSKUs { get; init; } = [];
    public bool HasStock { get; init; }
    public int TotalStock { get; init; }

    public static ProductSearchDocument FromProduct(Product product)
    {
        var variants = product.Variants?.ToArray() ?? [];
        var categories = product.Categories?.ToArray() ?? [];
        var attributes = product.Attributes?.ToArray() ?? [];
        var images = product.Images?.ToArray() ?? [];

        var priceRange = CalculatePriceRange(product.BasePrice, variants);
        var attributeFilters = BuildAttributeFilters(attributes, variants);
        var totalStock = variants.Sum(v => v.StockQuantity);
        var hasStock = variants.Any(v => v.StockQuantity > 0);

        return new ProductSearchDocument
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description ?? "",
            SKU = product.Sku ?? "",
            Slug = product.Slug?.Value ?? "",
            BasePrice = product.BasePrice?.Amount ?? 0,
            
            Categories = categories.Select(pc => new ProductSearchCategory
            {
                Id = pc.CategoryId,
                Name = "Category", // Will need to be loaded from navigation or DTO
                Slug = "",
                ParentId = null,
                HierarchyPath = "Category"
            }).ToArray(),
            
            Attributes = attributes.Select(pa => new ProductSearchAttribute
            {
                Name = $"Attribute_{pa.AttributeId}", // Using AttributeId as placeholder
                DisplayName = $"Attribute_{pa.AttributeId}",
                Value = pa.Values.ContainsKey("value") ? pa.Values["value"]?.ToString() ?? "" : "",
                Type = "String",
                Filterable = true
            }).ToArray(),
            
            Variants = variants.Select(v => new ProductSearchVariant
            {
                Id = v.Id,
                SKU = v.Sku ?? "",
                Price = v.Price?.Amount ?? product.BasePrice?.Amount ?? 0,
                Stock = v.StockQuantity,
                IsActive = v.IsActive,
                Attributes = v.VariantAttributes?.Select(va => new ProductSearchVariantAttribute
                {
                    Name = $"VariantAttribute_{va.AttributeId}",
                    Value = va.Value.ContainsKey("value") ? va.Value["value"]?.ToString() ?? "" : ""
                }).ToArray() ?? [],
                Images = v.Images?.Select(img => new ProductSearchImage
                {
                    Url = img.ImageKey,
                    AltText = img.AltText ?? "",
                    Order = img.DisplayOrder
                }).ToArray() ?? []
            }).ToArray(),
            
            Images = images.Select((img, index) => new ProductSearchImage
            {
                Url = img.ImageKey, // Using ImageKey as URL for now
                AltText = img.AltText ?? "",
                Order = img.DisplayOrder
            }).ToArray(),
            
            Status = "Active", // Product doesn't have Status enum, using IsActive
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            
            PriceRange = priceRange,
            AttributeFilters = attributeFilters,
            CategoryIds = categories.Select(pc => pc.CategoryId).ToArray(),
            VariantSKUs = variants.Where(v => !string.IsNullOrEmpty(v.Sku)).Select(v => v.Sku!).ToArray(),
            HasStock = hasStock,
            TotalStock = totalStock
        };
    }

    public static ProductSearchDocument FromProductDto(ProductDetailDto productDto)
    {
        var variants = productDto.Variants?.ToArray() ?? [];
        var categories = productDto.Categories?.ToArray() ?? [];
        var attributes = productDto.Attributes?.ToArray() ?? [];
        var images = productDto.Images?.ToArray() ?? [];

        var priceRange = CalculatePriceRangeFromDto(productDto.BasePrice, variants);
        var attributeFilters = BuildAttributeFiltersFromDto(attributes, variants);
        var totalStock = variants.Sum(v => v.StockQuantity);
        var hasStock = variants.Any(v => v.StockQuantity > 0);

        return new ProductSearchDocument
        {
            Id = productDto.Id,
            Name = productDto.Name,
            Description = productDto.Description ?? "",
            SKU = productDto.Sku ?? "",
            Slug = productDto.Slug ?? "",
            BasePrice = productDto.BasePrice,
            
            Categories = categories.Select(c => new ProductSearchCategory
            {
                Id = c.Id,
                Name = c.Name ?? "Category",
                Slug = c.Slug ?? "",
                ParentId = c.ParentId,
                HierarchyPath = c.Name ?? "Category"
            }).ToArray(),
            
            Attributes = attributes.Select(pa => new ProductSearchAttribute
            {
                Name = pa.AttributeName ?? "Unknown",
                DisplayName = pa.AttributeDisplayName ?? pa.AttributeName ?? "Unknown",
                Value = pa.Values.ContainsKey("value") ? pa.Values["value"]?.ToString() ?? "" : "",
                Type = "String", // TODO: Add type to ProductAttributeDto
                Filterable = true // TODO: Add filterable to ProductAttributeDto
            }).ToArray(),
            
            Variants = variants.Select(v => new ProductSearchVariant
            {
                Id = v.Id,
                SKU = v.Sku ?? "",
                Price = v.Price,
                Stock = v.StockQuantity,
                IsActive = v.IsActive,
                Attributes = v.Attributes?.Select(va => new ProductSearchVariantAttribute
                {
                    Name = va.AttributeName ?? "Unknown",
                    Value = va.Value.ContainsKey("value") ? va.Value["value"]?.ToString() ?? "" : ""
                }).ToArray() ?? [],
                Images = v.Images?.Select(img => new ProductSearchImage
                {
                    Url = img.ImageKey ?? "",
                    AltText = img.AltText ?? "",
                    Order = img.DisplayOrder
                }).ToArray() ?? []
            }).ToArray(),
            
            Images = images.Select(img => new ProductSearchImage
            {
                Url = img.ImageKey ?? "",
                AltText = img.AltText ?? "",
                Order = img.DisplayOrder
            }).ToArray(),
            
            Status = "Active",
            IsActive = productDto.IsActive,
            CreatedAt = productDto.CreatedAt,
            UpdatedAt = productDto.UpdatedAt,
            
            PriceRange = priceRange,
            AttributeFilters = attributeFilters,
            CategoryIds = categories.Select(c => c.Id).ToArray(),
            VariantSKUs = variants.Where(v => !string.IsNullOrEmpty(v.Sku)).Select(v => v.Sku!).ToArray(),
            HasStock = hasStock,
            TotalStock = totalStock
        };
    }

    private static ProductSearchPriceRange CalculatePriceRange(Domain.Sales.ValueObjects.Money? basePrice, ProductVariant[] variants)
    {
        var basePriceAmount = basePrice?.Amount ?? 0;
        var prices = new List<decimal> { basePriceAmount };
        
        if (variants.Length > 0)
        {
            prices.AddRange(variants.Where(v => v.Price != null).Select(v => v.Price!.Amount));
        }

        return new ProductSearchPriceRange
        {
            Min = prices.Min(),
            Max = prices.Max()
        };
    }

    private static Dictionary<string, string[]> BuildAttributeFilters(ProductAttribute[] attributes, ProductVariant[] variants)
    {
        var filters = new Dictionary<string, string[]>();

        foreach (var attr in attributes)
        {
            if (attr.Values.ContainsKey("value"))
            {
                var value = attr.Values["value"]?.ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    var key = $"Attribute_{attr.AttributeId}"; // Using AttributeId as key for now
                    if (!filters.ContainsKey(key))
                        filters[key] = [];
                    
                    var values = filters[key].ToList();
                    if (!values.Contains(value))
                        values.Add(value);
                    
                    filters[key] = values.ToArray();
                }
            }
        }

        // Note: Variant attributes would need similar handling but requires navigation properties
        return filters;
    }
}

public class ProductSearchCategory
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public string Slug { get; init; } = "";
    public Guid? ParentId { get; init; }
    public string HierarchyPath { get; init; } = "";
}

public class ProductSearchAttribute
{
    public string Name { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public string Value { get; init; } = "";
    public string Type { get; init; } = "";
    public bool Filterable { get; init; }
}

public class ProductSearchVariant
{
    public Guid Id { get; init; }
    public string SKU { get; init; } = "";
    public decimal Price { get; init; }
    public int Stock { get; init; }
    public bool IsActive { get; init; }
    public ProductSearchVariantAttribute[] Attributes { get; init; } = [];
    public ProductSearchImage[] Images { get; init; } = [];
}

public class ProductSearchVariantAttribute
{
    public string Name { get; init; } = "";
    public string Value { get; init; } = "";
}

public class ProductSearchImage
{
    public string Url { get; init; } = "";
    public string AltText { get; init; } = "";
    public int Order { get; init; }
}

public class ProductSearchPriceRange
{
    public decimal Min { get; init; }
    public decimal Max { get; init; }
}

// Helper methods for DTO-based document creation
partial class ProductSearchDocument
{
    private static ProductSearchPriceRange CalculatePriceRangeFromDto(decimal basePrice, ProductVariantDto[] variants)
    {
        var prices = new List<decimal> { basePrice };
        
        if (variants.Length > 0)
        {
            prices.AddRange(variants.Select(v => v.Price));
        }

        return new ProductSearchPriceRange
        {
            Min = prices.Min(),
            Max = prices.Max()
        };
    }

    private static Dictionary<string, string[]> BuildAttributeFiltersFromDto(ProductAttributeDto[] attributes, ProductVariantDto[] variants)
    {
        var filters = new Dictionary<string, string[]>();

        // Process product attributes
        foreach (var attr in attributes)
        {
            if (attr.Values.ContainsKey("value"))
            {
                var value = attr.Values["value"]?.ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    var key = attr.AttributeName ?? $"Attribute_{attr.AttributeId}";
                    if (!filters.ContainsKey(key))
                        filters[key] = [];
                    
                    var values = filters[key].ToList();
                    if (!values.Contains(value))
                        values.Add(value);
                    
                    filters[key] = values.ToArray();
                }
            }
        }

        // Process variant attributes
        foreach (var variant in variants)
        {
            if (variant.Attributes != null)
            {
                foreach (var attr in variant.Attributes)
                {
                    if (attr.Value.ContainsKey("value"))
                    {
                        var value = attr.Value["value"]?.ToString();
                        if (!string.IsNullOrEmpty(value))
                        {
                            var key = attr.AttributeName ?? $"VariantAttribute_{attr.AttributeId}";
                            if (!filters.ContainsKey(key))
                                filters[key] = [];
                            
                            var values = filters[key].ToList();
                            if (!values.Contains(value))
                                values.Add(value);
                            
                            filters[key] = values.ToArray();
                        }
                    }
                }
            }
        }

        return filters;
    }
}