using Bogus;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Attributes.DeleteAttribute.V1;

public static class DeleteAttributeTestDataV1
{
    private static readonly Faker _faker = new();

    /// <summary>
    /// Creates a valid attribute for deletion testing
    /// </summary>
    public static object CreateValidAttributeForDeletion(
        string? name = null,
        string? displayName = null,
        string? type = null,
        bool? filterable = null,
        bool? searchable = null,
        bool? isVariant = null,
        Dictionary<string, object>? configuration = null)
    {
        return new
        {
            Name = name ?? $"delete_test_{_faker.Random.AlphaNumeric(10).ToLower()}",
            DisplayName = displayName ?? $"Delete Test {_faker.Commerce.ProductAdjective()}",
            Type = type ?? "Text",
            Filterable = filterable ?? _faker.Random.Bool(),
            Searchable = searchable ?? _faker.Random.Bool(),
            IsVariant = isVariant ?? _faker.Random.Bool(),
            Configuration = configuration ?? new Dictionary<string, object>()
        };
    }

    /// <summary>
    /// Creates an attribute that is used by products (for conflict testing)
    /// </summary>
    public static object CreateAttributeUsedByProducts()
    {
        return new
        {
            Name = "size", // Commonly used attribute name
            DisplayName = "Product Size",
            Type = "Select",
            Filterable = true,
            Searchable = true,
            IsVariant = true,
            Configuration = new Dictionary<string, object>
            {
                ["options"] = new[] { "Small", "Medium", "Large" }
            }
        };
    }

    /// <summary>
    /// Edge cases for testing various scenarios
    /// </summary>
    public static class EdgeCases
    {
        public static object CreateAttributeWithUnicodeCharacters()
        {
            return new
            {
                Name = "café_münchën_™",
                DisplayName = "Café Münchën Attribute™",
                Type = "Text",
                Filterable = true,
                Searchable = true,
                IsVariant = false,
                Configuration = new Dictionary<string, object>()
            };
        }

        public static object CreateAttributeWithComplexConfiguration()
        {
            return new
            {
                Name = "complex_config",
                DisplayName = "Complex Configuration Attribute",
                Type = "Select",
                Filterable = true,
                Searchable = true,
                IsVariant = true,
                Configuration = new Dictionary<string, object>
                {
                    ["options"] = new[] { "Option1", "Option2", "Option3" },
                    ["allowMultiple"] = true,
                    ["defaultValue"] = "Option1",
                    ["metadata"] = new Dictionary<string, object>
                    {
                        ["color"] = "#FF0000",
                        ["priority"] = 1
                    }
                }
            };
        }

        public static object CreateAttributeWithLongName()
        {
            return new
            {
                Name = new string('a', 100), // Maximum valid length
                DisplayName = "Long Name Attribute",
                Type = "Text",
                Filterable = false,
                Searchable = false,
                IsVariant = false,
                Configuration = new Dictionary<string, object>()
            };
        }
    }

    /// <summary>
    /// Test scenarios for different attribute types
    /// </summary>
    public static class TypeSpecificCases
    {
        public static object CreateTextAttribute()
        {
            return new
            {
                Name = "text_attribute_delete",
                DisplayName = "Text Attribute for Deletion",
                Type = "Text",
                Filterable = false,
                Searchable = true,
                IsVariant = false,
                Configuration = new Dictionary<string, object>
                {
                    ["maxLength"] = 255
                }
            };
        }

        public static object CreateSelectAttribute()
        {
            return new
            {
                Name = "select_attribute_delete",
                DisplayName = "Select Attribute for Deletion",
                Type = "Select",
                Filterable = true,
                Searchable = true,
                IsVariant = true,
                Configuration = new Dictionary<string, object>
                {
                    ["options"] = new[] { "Red", "Blue", "Green" }
                }
            };
        }

        public static object CreateColorAttribute()
        {
            return new
            {
                Name = "color_attribute_delete",
                DisplayName = "Color Attribute for Deletion",
                Type = "Color",
                Filterable = true,
                Searchable = false,
                IsVariant = true,
                Configuration = new Dictionary<string, object>
                {
                    ["format"] = "hex"
                }
            };
        }

        public static object CreateNumberAttribute()
        {
            return new
            {
                Name = "number_attribute_delete",
                DisplayName = "Number Attribute for Deletion",
                Type = "Number",
                Filterable = true,
                Searchable = false,
                IsVariant = false,
                Configuration = new Dictionary<string, object>
                {
                    ["min"] = 0,
                    ["max"] = 1000,
                    ["decimal"] = 2
                }
            };
        }

        public static object CreateBooleanAttribute()
        {
            return new
            {
                Name = "boolean_attribute_delete",
                DisplayName = "Boolean Attribute for Deletion",
                Type = "Boolean",
                Filterable = true,
                Searchable = false,
                IsVariant = false,
                Configuration = new Dictionary<string, object>
                {
                    ["trueLabel"] = "Yes",
                    ["falseLabel"] = "No"
                }
            };
        }

        public static object CreateDateAttribute()
        {
            return new
            {
                Name = "date_attribute_delete",
                DisplayName = "Date Attribute for Deletion",
                Type = "Date",
                Filterable = true,
                Searchable = false,
                IsVariant = false,
                Configuration = new Dictionary<string, object>
                {
                    ["format"] = "yyyy-MM-dd"
                }
            };
        }

        public static object CreateDimensionsAttribute()
        {
            return new
            {
                Name = "dimensions_attribute_delete",
                DisplayName = "Dimensions Attribute for Deletion",
                Type = "Dimensions",
                Filterable = false,
                Searchable = false,
                IsVariant = false,
                Configuration = new Dictionary<string, object>
                {
                    ["unit"] = "cm",
                    ["dimensions"] = new[] { "Length", "Width", "Height" }
                }
            };
        }

        public static object CreateWeightAttribute()
        {
            return new
            {
                Name = "weight_attribute_delete",
                DisplayName = "Weight Attribute for Deletion",
                Type = "Weight",
                Filterable = true,
                Searchable = false,
                IsVariant = false,
                Configuration = new Dictionary<string, object>
                {
                    ["unit"] = "kg",
                    ["precision"] = 2
                }
            };
        }
    }

    /// <summary>
    /// Helper methods for creating products that use specific attributes
    /// </summary>
    public static class ProductHelpers
    {
        public static object CreateProductWithAttribute(Guid attributeId, string attributeName)
        {
            return new
            {
                Name = $"Product using {attributeName}",
                Description = $"Product that uses the {attributeName} attribute",
                Price = _faker.Random.Decimal(1, 1000),
                Sku = $"SKU-{_faker.Random.AlphaNumeric(8).ToUpper()}",
                CategoryId = Guid.NewGuid(),
                IsActive = true,
                Attributes = new[]
                {
                    new
                    {
                        AttributeId = attributeId,
                        Value = "Test Value"
                    }
                }
            };
        }

        public static object CreateProductVariantWithAttribute(Guid attributeId, string attributeName)
        {
            return new
            {
                Name = $"Variant using {attributeName}",
                Sku = $"VAR-{_faker.Random.AlphaNumeric(8).ToUpper()}",
                Price = _faker.Random.Decimal(1, 1000),
                Stock = _faker.Random.Int(0, 100),
                Attributes = new[]
                {
                    new
                    {
                        AttributeId = attributeId,
                        Value = "Variant Value"
                    }
                }
            };
        }
    }
}