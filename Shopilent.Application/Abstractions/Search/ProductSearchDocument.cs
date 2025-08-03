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
    public string[] CategorySlugs { get; init; } = [];
    public string[] VariantSKUs { get; init; } = [];
    public bool HasStock { get; init; }
    public int TotalStock { get; init; }
    
    public Dictionary<string, string[]> FlatAttributes { get; init; } = new();


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
            CategorySlugs = categories.Where(c => !string.IsNullOrEmpty(c.Slug)).Select(c => c.Slug!).ToArray(),
            VariantSKUs = variants.Where(v => !string.IsNullOrEmpty(v.Sku)).Select(v => v.Sku!).ToArray(),
            HasStock = hasStock,
            TotalStock = totalStock,
            
            FlatAttributes = BuildFlatAttributesFromDto(attributes, variants)
        };
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

    private static Dictionary<string, string[]> BuildFlatAttributesFromDto(ProductAttributeDto[] attributes, ProductVariantDto[] variants)
    {
        var flatAttributes = new Dictionary<string, string[]>();

        foreach (var attr in attributes)
        {
            var attributeName = attr.AttributeName?.ToLowerInvariant();
            if (!string.IsNullOrEmpty(attributeName) && attr.Values?.Any() == true)
            {
                string value = null;
                
                if (attr.Values.ContainsKey("value"))
                    value = attr.Values["value"]?.ToString();
                else if (attr.Values.ContainsKey("Value"))
                    value = attr.Values["Value"]?.ToString();
                else if (attr.Values.ContainsKey(attributeName))
                    value = attr.Values[attributeName]?.ToString();
                else
                    value = attr.Values.FirstOrDefault().Value?.ToString();
                
                if (!string.IsNullOrEmpty(value))
                {
                    flatAttributes[$"attr-{attributeName}"] = new[] { value };
                }
            }
        }

        foreach (var variant in variants)
        {
            if (variant.Attributes != null)
            {
                foreach (var variantAttr in variant.Attributes)
                {
                    var attributeName = variantAttr.AttributeName?.ToLowerInvariant();
                    if (!string.IsNullOrEmpty(attributeName) && variantAttr.Value?.Any() == true)
                    {
                        string value = null;
                        
                        if (variantAttr.Value.ContainsKey("value"))
                            value = variantAttr.Value["value"]?.ToString();
                        else if (variantAttr.Value.ContainsKey("Value"))
                            value = variantAttr.Value["Value"]?.ToString();
                        else if (variantAttr.Value.ContainsKey(attributeName))
                            value = variantAttr.Value[attributeName]?.ToString();
                        else
                            value = variantAttr.Value.FirstOrDefault().Value?.ToString();
                        
                        if (!string.IsNullOrEmpty(value))
                        {
                            var prefixedAttributeName = $"attr-{attributeName}";
                            if (!flatAttributes.ContainsKey(prefixedAttributeName))
                            {
                                flatAttributes[prefixedAttributeName] = new[] { value };
                            }
                            else
                            {
                                var existingValues = flatAttributes[prefixedAttributeName].ToList();
                                if (!existingValues.Contains(value))
                                {
                                    existingValues.Add(value);
                                    flatAttributes[prefixedAttributeName] = existingValues.ToArray();
                                }
                            }
                        }
                    }
                }
            }
        }

        return flatAttributes;
    }
}