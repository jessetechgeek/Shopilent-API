using Bogus;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Attributes.CreateAttribute.V1;

public static class CreateAttributeTestDataV1
{
    private static readonly Faker _faker = new();

    private static readonly string[] ValidAttributeTypes =
    {
        "Text", "Number", "Boolean", "Select", "Color", "Date", "Dimensions", "Weight"
    };

    // Core valid request generator
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

    // Validation test cases - Name validation
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

    // Validation test cases - DisplayName validation
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

    // Validation test cases - Type validation
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

    // Boundary value testing
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
    }

    // Edge cases
    public static class EdgeCases
    {
        public static object CreateRequestWithUnicodeCharacters() => new
        {
            Name = "café_münchën_™",
            DisplayName = "Café Münchën Attribute™",
            Type = _faker.Random.ArrayElement(ValidAttributeTypes),
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>
            {
                { "unicode_value", "Ürünümüz için değer" }
            }
        };

        public static object CreateRequestWithSpecialCharacters() => new
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

        public static object CreateRequestWithComplexConfiguration() => new
        {
            Name = _faker.Random.AlphaNumeric(10).ToLower(),
            DisplayName = _faker.Commerce.ProductAdjective(),
            Type = "Select",
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>
            {
                { "options", new List<string> { "Option 1", "Option 2", "Option 3" } },
                { "multiple_selection", true },
                { "default_value", "Option 1" },
                { "validation_rules", new Dictionary<string, object> { { "required", true } } }
            }
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
    }

    // Type-specific test cases
    public static class TypeSpecificCases
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
                { "placeholder", "Enter text value" }
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
                { "decimal_places", 2 }
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
                { "false_label", "No" }
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
                { "multiple_selection", false }
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
                { "allow_custom", true }
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
                { "max_weight", 1000.0 }
            }
        };
    }

    // Case sensitivity tests
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
}