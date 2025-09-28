using Bogus;
using Shopilent.Domain.Catalog.Enums;
using Shopilent.Domain.Common.Models;

namespace Shopilent.API.IntegrationTests.Common.TestData;

public static class AttributeTestDataV1
{
    private static readonly Faker _faker = new();

    private static readonly string[] ValidAttributeTypes =
    {
        "Text", "Number", "Boolean", "Select", "Color", "Date", "Dimensions", "Weight"
    };

    /// <summary>
    /// Core attribute creation methods for all test scenarios
    /// </summary>
    public static class Creation
    {
        /// <summary>
        /// Creates a valid attribute request with customizable parameters
        /// </summary>
        public static object CreateValidRequest(
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
                Name = name ?? _faker.Random.AlphaNumeric(10).ToLower(),
                DisplayName = displayName ?? _faker.Commerce.ProductAdjective(),
                Type = type ?? _faker.Random.ArrayElement(ValidAttributeTypes),
                Filterable = filterable ?? _faker.Random.Bool(),
                Searchable = searchable ?? _faker.Random.Bool(),
                IsVariant = isVariant ?? _faker.Random.Bool(),
                Configuration = configuration ?? new Dictionary<string, object>
                {
                    { "default_value", _faker.Random.Word() },
                    { "placeholder", _faker.Lorem.Sentence(3) }
                }
            };
        }

        /// <summary>
        /// Creates a valid attribute for deletion testing with unique naming
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
        /// Creates an attribute for seeding test data
        /// </summary>
        public static object CreateAttributeForSeeding(
            string? name = null,
            string? displayName = null,
            string? type = null,
            bool? filterable = null,
            bool? searchable = null,
            bool? isVariant = null)
        {
            var attributeType = type ?? _faker.PickRandom<AttributeType>().ToString();

            return new
            {
                Name = name ?? _faker.Random.AlphaNumeric(8).ToLower(),
                DisplayName = displayName ?? _faker.Commerce.ProductAdjective(),
                Type = attributeType,
                Configuration = new Dictionary<string, object>(),
                Filterable = filterable ?? _faker.Random.Bool(),
                Searchable = searchable ?? _faker.Random.Bool(),
                IsVariant = isVariant ?? _faker.Random.Bool()
            };
        }

        /// <summary>
        /// Creates multiple attributes for bulk testing scenarios
        /// </summary>
        public static List<object> CreateMultipleAttributesForSeeding(int count = 5)
        {
            var attributes = new List<object>();
            var types = Enum.GetValues<AttributeType>().Take(count).ToArray();

            for (int i = 0; i < count; i++)
            {
                var type = types.Length > i ? types[i] : _faker.PickRandom<AttributeType>();
                attributes.Add(CreateAttributeForSeeding(
                    name: $"test_attr_{i}_{_faker.Random.AlphaNumeric(4)}",
                    displayName: $"Test Attribute {i + 1}",
                    type: type.ToString(),
                    filterable: i % 2 == 0,
                    searchable: i % 3 == 0,
                    isVariant: i % 4 == 0
                ));
            }

            return attributes;
        }

        /// <summary>
        /// Creates an attribute that is commonly used by products (for conflict testing)
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
    }

    /// <summary>
    /// Validation test cases for various field validations
    /// </summary>
    public static class Validation
    {
        // Name validation
        public static object CreateRequestWithEmptyName() => new
        {
            Name = "",
            DisplayName = _faker.Commerce.ProductAdjective(),
            Type = _faker.Random.ArrayElement(ValidAttributeTypes),
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>()
        };

        public static object CreateRequestWithNullName() => new
        {
            Name = (string?)null,
            DisplayName = _faker.Commerce.ProductAdjective(),
            Type = _faker.Random.ArrayElement(ValidAttributeTypes),
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>()
        };

        public static object CreateRequestWithWhitespaceName() => new
        {
            Name = "   ",
            DisplayName = _faker.Commerce.ProductAdjective(),
            Type = _faker.Random.ArrayElement(ValidAttributeTypes),
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>()
        };

        public static object CreateRequestWithLongName() => new
        {
            Name = new string('A', 101), // Exceeds 100 character limit
            DisplayName = _faker.Commerce.ProductAdjective(),
            Type = _faker.Random.ArrayElement(ValidAttributeTypes),
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>()
        };

        // DisplayName validation
        public static object CreateRequestWithEmptyDisplayName() => new
        {
            Name = _faker.Random.AlphaNumeric(10).ToLower(),
            DisplayName = "",
            Type = _faker.Random.ArrayElement(ValidAttributeTypes),
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>()
        };

        public static object CreateRequestWithNullDisplayName() => new
        {
            Name = _faker.Random.AlphaNumeric(10).ToLower(),
            DisplayName = (string?)null,
            Type = _faker.Random.ArrayElement(ValidAttributeTypes),
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>()
        };

        public static object CreateRequestWithWhitespaceDisplayName() => new
        {
            Name = _faker.Random.AlphaNumeric(10).ToLower(),
            DisplayName = "   ",
            Type = _faker.Random.ArrayElement(ValidAttributeTypes),
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>()
        };

        public static object CreateRequestWithLongDisplayName() => new
        {
            Name = _faker.Random.AlphaNumeric(10).ToLower(),
            DisplayName = new string('B', 101), // Exceeds 100 character limit
            Type = _faker.Random.ArrayElement(ValidAttributeTypes),
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>()
        };

        // Type validation
        public static object CreateRequestWithEmptyType() => new
        {
            Name = _faker.Random.AlphaNumeric(10).ToLower(),
            DisplayName = _faker.Commerce.ProductAdjective(),
            Type = "",
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>()
        };

        public static object CreateRequestWithInvalidType() => new
        {
            Name = _faker.Random.AlphaNumeric(10).ToLower(),
            DisplayName = _faker.Commerce.ProductAdjective(),
            Type = "InvalidType",
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>()
        };
    }

    /// <summary>
    /// Boundary value testing for limits and edge values
    /// </summary>
    public static class BoundaryTests
    {
        public static object CreateRequestWithMaximumNameLength() => new
        {
            Name = new string('a', 100), // Exactly 100 characters
            DisplayName = _faker.Commerce.ProductAdjective(),
            Type = _faker.Random.ArrayElement(ValidAttributeTypes),
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>()
        };

        public static object CreateRequestWithMaximumDisplayNameLength() => new
        {
            Name = _faker.Random.AlphaNumeric(10).ToLower(),
            DisplayName = new string('b', 100), // Exactly 100 characters
            Type = _faker.Random.ArrayElement(ValidAttributeTypes),
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>()
        };

        public static object CreateRequestWithMinimumValidName() => new
        {
            Name = "a", // Single character
            DisplayName = _faker.Commerce.ProductAdjective(),
            Type = _faker.Random.ArrayElement(ValidAttributeTypes),
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>()
        };

        public static object CreateRequestWithMinimumValidDisplayName() => new
        {
            Name = _faker.Random.AlphaNumeric(10).ToLower(),
            DisplayName = "A", // Single character
            Type = _faker.Random.ArrayElement(ValidAttributeTypes),
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>()
        };
    }

    /// <summary>
    /// Edge cases for special scenarios
    /// </summary>
    public static class EdgeCases
    {
        public static object CreateRequestWithUnicodeCharacters() => new
        {
            Name = "café_münchën_",
            DisplayName = "Café Münchën Attribute™",
            Type = _faker.Random.ArrayElement(ValidAttributeTypes),
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>
            {
                { "unicode_value", "Ürünümüz için değer" },
                { "description", "测试属性 with émojis 🎉" },
                { "emoji", "🛍️🔥⭐" }
            }
        };

        // For Create tests that expect the ™ symbol in name
        public static object CreateRequestWithUnicodeCharactersForCreate() => new
        {
            Name = "café_münchën_™",
            DisplayName = "Café Münchën Attribute™",
            Type = _faker.Random.ArrayElement(ValidAttributeTypes),
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>
            {
                { "unicode_value", "Ürünümüz için değer" },
                { "description", "测试属性 with émojis 🎉" },
                { "emoji", "🛍️🔥⭐" }
            }
        };

        public static object CreateRequestWithSpecialCharacters() => new
        {
            Name = "test_attribute_123",
            DisplayName = "Special-Chars_123!@#",
            Type = _faker.Random.ArrayElement(ValidAttributeTypes),
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>
            {
                { "special_chars", "!@#$%^&*()" }
            }
        };

        // For Create tests that expect different display name
        public static object CreateRequestWithSpecialCharactersForCreate() => new
        {
            Name = "test_attribute_123",
            DisplayName = "Test Attribute @ 2024!",
            Type = _faker.Random.ArrayElement(ValidAttributeTypes),
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>
            {
                { "special_chars", "!@#$%^&*()" }
            }
        };

        public static object CreateRequestWithEmptyConfiguration() => new
        {
            Name = _faker.Random.AlphaNumeric(10).ToLower(),
            DisplayName = _faker.Commerce.ProductAdjective(),
            Type = _faker.Random.ArrayElement(ValidAttributeTypes),
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>()
        };

        public static object CreateRequestWithNullConfiguration() => new
        {
            Name = _faker.Random.AlphaNumeric(10).ToLower(),
            DisplayName = _faker.Commerce.ProductAdjective(),
            Type = _faker.Random.ArrayElement(ValidAttributeTypes),
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = (Dictionary<string, object>?)null
        };

        public static object CreateRequestWithComplexConfiguration() => new
        {
            Name = "complex_config_attribute",
            DisplayName = "Complex Configuration Attribute",
            Type = "Select",
            Filterable = true,
            Searchable = false,
            IsVariant = true,
            Configuration = new Dictionary<string, object>
            {
                { "options", new[] { "Option 1", "Option 2", "Option 3" } },
                { "multiple", false },
                { "multiple_selection", true },
                { "searchable", true },
                { "metadata", new { category = "size", priority = 1 } },
                { "array_value", new[] { "item1", "item2", "item3" } },
                { "number_value", 42 },
                { "string_value", "test_string" },
                { "boolean_value", true },
                { "default_value", "Option 1" },
                { "nested_object", new { nested_number = 123, nested_string = "nested_value" } },
                { "validation_rules", new { required = true } }
            }
        };
    }

    /// <summary>
    /// Type-specific test cases for different attribute types
    /// </summary>
    public static class TypeSpecific
    {
        public static object CreateTextAttributeRequest() => new
        {
            Name = "text_attribute",
            DisplayName = "Text Attribute",
            Type = "Text",
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>
            {
                { "max_length", 255 },
                { "maxLength", 255 },
                { "placeholder", "Enter text value" },
                { "multiline", false }
            }
        };

        public static object CreateNumberAttributeRequest() => new
        {
            Name = "number_attribute",
            DisplayName = "Number Attribute",
            Type = "Number",
            Filterable = true,
            Searchable = false,
            IsVariant = true,
            Configuration = new Dictionary<string, object>
            {
                { "min_value", 0 },
                { "max_value", 1000 },
                { "decimal_places", 2 },
                { "min", 0 },
                { "max", 100 },
                { "decimals", 2 }
            }
        };

        public static object CreateBooleanAttributeRequest() => new
        {
            Name = "boolean_attribute",
            DisplayName = "Boolean Attribute",
            Type = "Boolean",
            Filterable = true,
            Searchable = false,
            IsVariant = false,
            Configuration = new Dictionary<string, object>
            {
                { "true_label", "Yes" },
                { "false_label", "No" },
                { "trueLabel", "Yes" },
                { "falseLabel", "No" },
                { "defaultValue", false }
            }
        };

        public static object CreateSelectAttributeRequest() => new
        {
            Name = "select_attribute",
            DisplayName = "Select Attribute",
            Type = "Select",
            Filterable = true,
            Searchable = false,
            IsVariant = true,
            Configuration = new Dictionary<string, object>
            {
                { "options", new List<string> { "Small", "Medium", "Large", "XL" } },
                { "multiple_selection", false },
                { "multiple", false },
                { "allowMultiple", true },
                { "defaultValue", "Small" },
                { "searchable", true }
            }
        };

        public static object CreateColorAttributeRequest() => new
        {
            Name = "color_attribute",
            DisplayName = "Color Attribute",
            Type = "Color",
            Filterable = true,
            Searchable = false,
            IsVariant = true,
            Configuration = new Dictionary<string, object>
            {
                { "color_format", "hex" },
                { "format", "hex" },
                { "allow_custom", true },
                { "allowTransparency", false }
            }
        };

        public static object CreateDateAttributeRequest() => new
        {
            Name = "date_attribute",
            DisplayName = "Date Attribute",
            Type = "Date",
            Filterable = true,
            Searchable = false,
            IsVariant = false,
            Configuration = new Dictionary<string, object>
            {
                { "date_format", "yyyy-MM-dd" },
                { "format", "yyyy-MM-dd" },
                { "min_date", "2020-01-01" },
                { "max_date", "2030-12-31" }
            }
        };

        public static object CreateDimensionsAttributeRequest() => new
        {
            Name = "dimensions_attribute",
            DisplayName = "Dimensions Attribute",
            Type = "Dimensions",
            Filterable = false,
            Searchable = false,
            IsVariant = true,
            Configuration = new Dictionary<string, object>
            {
                { "unit", "cm" },
                { "dimensions", new List<string> { "length", "width", "height" } }
            }
        };

        public static object CreateWeightAttributeRequest() => new
        {
            Name = "weight_attribute",
            DisplayName = "Weight Attribute",
            Type = "Weight",
            Filterable = true,
            Searchable = false,
            IsVariant = true,
            Configuration = new Dictionary<string, object>
            {
                { "unit", "kg" },
                { "min_weight", 0.1 },
                { "max_weight", 1000.0 },
                { "precision", 2 }
            }
        };

        /// <summary>
        /// Creates attributes of all types for comprehensive testing
        /// </summary>
        public static List<object> CreateAttributesOfAllTypes()
        {
            var attributes = new List<object>();
            var attributeTypes = Enum.GetValues<AttributeType>();

            foreach (var type in attributeTypes)
            {
                attributes.Add(Creation.CreateAttributeForSeeding(
                    name: $"{type.ToString().ToLower()}_attribute",
                    displayName: $"{type} Attribute",
                    type: type.ToString(),
                    filterable: type == AttributeType.Select || type == AttributeType.Color,
                    searchable: type == AttributeType.Text,
                    isVariant: type == AttributeType.Color || type == AttributeType.Select
                ));
            }

            return attributes;
        }
    }

    /// <summary>
    /// Case sensitivity test scenarios
    /// </summary>
    public static class CaseSensitivityTests
    {
        public static object CreateRequestWithLowercaseType() => new
        {
            Name = _faker.Random.AlphaNumeric(10).ToLower(),
            DisplayName = _faker.Commerce.ProductAdjective(),
            Type = "text", // lowercase
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>()
        };

        public static object CreateRequestWithUppercaseType() => new
        {
            Name = _faker.Random.AlphaNumeric(10).ToLower(),
            DisplayName = _faker.Commerce.ProductAdjective(),
            Type = "TEXT", // uppercase
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>()
        };

        public static object CreateRequestWithMixedCaseType() => new
        {
            Name = _faker.Random.AlphaNumeric(10).ToLower(),
            DisplayName = _faker.Commerce.ProductAdjective(),
            Type = "BoOlEaN", // mixed case
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>()
        };
    }

    /// <summary>
    /// Update-specific test scenarios for UpdateAttribute operations
    /// </summary>
    public static class UpdateScenarios
    {
        /// <summary>
        /// Creates a valid update request (excludes Name and Type as they're typically immutable)
        /// </summary>
        public static object CreateValidUpdateRequest(
            string? displayName = null,
            bool? filterable = null,
            bool? searchable = null,
            bool? isVariant = null,
            Dictionary<string, object>? configuration = null)
        {
            return new
            {
                DisplayName = displayName ?? _faker.Commerce.ProductAdjective(),
                Filterable = filterable ?? _faker.Random.Bool(),
                Searchable = searchable ?? _faker.Random.Bool(),
                IsVariant = isVariant ?? _faker.Random.Bool(),
                Configuration = configuration ?? new Dictionary<string, object>
                {
                    { "default_value", _faker.Random.Word() },
                    { "placeholder", _faker.Lorem.Sentence(3) },
                    { "options", new[] { "Option1", "Option2", "Option3" } }
                }
            };
        }

        /// <summary>
        /// Different property combinations for update testing
        /// </summary>
        public static class PropertyCombinations
        {
            public static object CreateRequestAllFilterableSearchableVariant() => new
            {
                DisplayName = "All True Attribute",
                Filterable = true,
                Searchable = true,
                IsVariant = true,
                Configuration = new Dictionary<string, object>
                {
                    { "filter_type", "multiselect" },
                    { "search_weight", 1.0 },
                    { "variant_display", "color_swatch" }
                }
            };

            public static object CreateRequestAllFalseFlags() => new
            {
                DisplayName = "All False Attribute",
                Filterable = false,
                Searchable = false,
                IsVariant = false,
                Configuration = new Dictionary<string, object>
                {
                    { "display_only", true },
                    { "admin_note", "Internal use only" }
                }
            };

            public static object CreateRequestFilterableOnly() => new
            {
                DisplayName = "Filterable Only",
                Filterable = true,
                Searchable = false,
                IsVariant = false,
                Configuration = new Dictionary<string, object>
                {
                    { "filter_type", "range" },
                    { "min_value", 0 },
                    { "max_value", 100 }
                }
            };

            public static object CreateRequestSearchableOnly() => new
            {
                DisplayName = "Searchable Only",
                Filterable = false,
                Searchable = true,
                IsVariant = false,
                Configuration = new Dictionary<string, object>
                {
                    { "search_boost", 2.0 },
                    { "search_analyzer", "standard" }
                }
            };

            public static object CreateRequestVariantOnly() => new
            {
                DisplayName = "Variant Only",
                Filterable = false,
                Searchable = false,
                IsVariant = true,
                Configuration = new Dictionary<string, object>
                {
                    { "variant_type", "size" },
                    { "display_order", 1 }
                }
            };
        }
    }

    /// <summary>
    /// Helper methods for creating products that use specific attributes (for delete conflict testing)
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

    /// <summary>
    /// DataTable-specific test scenarios (separate from main DataTable file for simpler scenarios)
    /// </summary>
    public static class DataTable
    {
        /// <summary>
        /// Creates a simple DataTable request for basic testing
        /// </summary>
        public static DataTableRequest CreateSimpleRequest(
            int draw = 1,
            int start = 0,
            int length = 10,
            string? searchValue = null)
        {
            return new DataTableRequest
            {
                Draw = draw,
                Start = start,
                Length = length,
                Search = new DataTableSearch
                {
                    Value = searchValue ?? string.Empty,
                    Regex = false
                },
                Columns = new List<DataTableColumn>
                {
                    new() { Data = "name", Name = "name", Searchable = true, Orderable = true },
                    new() { Data = "displayName", Name = "displayName", Searchable = true, Orderable = true },
                    new() { Data = "type", Name = "type", Searchable = true, Orderable = true }
                },
                Order = new List<DataTableOrder>
                {
                    new() { Column = 0, Dir = "asc" }
                }
            };
        }
    }
}