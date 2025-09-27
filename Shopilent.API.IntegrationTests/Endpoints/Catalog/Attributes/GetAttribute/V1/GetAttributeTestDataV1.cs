using Bogus;
using Shopilent.Domain.Catalog.Enums;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Attributes.GetAttribute.V1;

public static class GetAttributeTestDataV1
{
    private static readonly Faker _faker = new();

    // Core valid request generator for creating test attributes
    public static object CreateValidAttributeRequest(
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
            DisplayName = displayName ?? _faker.Commerce.ProductName(),
            Type = type ?? "Text",
            Filterable = filterable ?? _faker.Random.Bool(),
            Searchable = searchable ?? _faker.Random.Bool(),
            IsVariant = isVariant ?? _faker.Random.Bool(),
            Configuration = configuration ?? new Dictionary<string, object>()
        };
    }

    // Different attribute types for testing
    public static class AttributeTypes
    {
        public static object CreateTextAttributeRequest() => new
        {
            Name = "text_get_test",
            DisplayName = "Text Get Test Attribute",
            Type = "Text",
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>
            {
                { "maxLength", 255 },
                { "multiline", false }
            }
        };

        public static object CreateSelectAttributeRequest() => new
        {
            Name = "select_get_test",
            DisplayName = "Select Get Test Attribute",
            Type = "Select",
            Filterable = true,
            Searchable = false,
            IsVariant = true,
            Configuration = new Dictionary<string, object>
            {
                { "options", new[] { "Small", "Medium", "Large" } },
                { "multiple", false }
            }
        };

        public static object CreateColorAttributeRequest() => new
        {
            Name = "color_get_test",
            DisplayName = "Color Get Test Attribute",
            Type = "Color",
            Filterable = true,
            Searchable = false,
            IsVariant = true,
            Configuration = new Dictionary<string, object>
            {
                { "format", "hex" },
                { "allowTransparency", false }
            }
        };

        public static object CreateNumberAttributeRequest() => new
        {
            Name = "number_get_test",
            DisplayName = "Number Get Test Attribute",
            Type = "Number",
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>
            {
                { "min", 0 },
                { "max", 100 },
                { "decimals", 2 }
            }
        };

        public static object CreateBooleanAttributeRequest() => new
        {
            Name = "boolean_get_test",
            DisplayName = "Boolean Get Test Attribute",
            Type = "Boolean",
            Filterable = true,
            Searchable = false,
            IsVariant = false,
            Configuration = new Dictionary<string, object>
            {
                { "defaultValue", false }
            }
        };
    }

    // Edge cases for testing
    public static class EdgeCases
    {
        public static object CreateAttributeWithUnicodeChars() => new
        {
            Name = "unicode_test_café",
            DisplayName = "Café Münchën Attribute™",
            Type = "Text",
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>
            {
                { "description", "测试属性 with émojis 🎉" }
            }
        };

        public static object CreateAttributeWithComplexConfig() => new
        {
            Name = "complex_config_test",
            DisplayName = "Complex Configuration Test",
            Type = "Select",
            Filterable = true,
            Searchable = false,
            IsVariant = true,
            Configuration = new Dictionary<string, object>
            {
                { "options", new object[] 
                    {
                        new { value = "xs", label = "Extra Small", color = "#ff0000" },
                        new { value = "sm", label = "Small", color = "#00ff00" },
                        new { value = "md", label = "Medium", color = "#0000ff" }
                    }
                },
                { "multiple", false },
                { "searchable", true },
                { "metadata", new { category = "size", priority = 1 } }
            }
        };

        public static object CreateAttributeWithEmptyConfig() => new
        {
            Name = "empty_config_test",
            DisplayName = "Empty Configuration Test",
            Type = "Text",
            Filterable = false,
            Searchable = false,
            IsVariant = false,
            Configuration = new Dictionary<string, object>()
        };
    }

    // Boundary tests
    public static class BoundaryTests
    {
        public static object CreateAttributeWithMaxNameLength() => new
        {
            Name = new string('a', 100), // Maximum allowed length
            DisplayName = "Max Name Length Test",
            Type = "Text",
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>()
        };

        public static object CreateAttributeWithMaxDisplayNameLength() => new
        {
            Name = "max_display_test",
            DisplayName = new string('B', 100), // Maximum allowed length
            Type = "Text",
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>()
        };

        public static object CreateAttributeWithMinNameLength() => new
        {
            Name = "a", // Minimum allowed length
            DisplayName = "Minimum Name Test",
            Type = "Text",
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>()
        };
    }
}
