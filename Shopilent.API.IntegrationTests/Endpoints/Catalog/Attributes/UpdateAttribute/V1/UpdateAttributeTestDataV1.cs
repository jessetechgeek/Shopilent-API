using Bogus;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Attributes.UpdateAttribute.V1;

public static class UpdateAttributeTestDataV1
{
    private static readonly Faker _faker = new();

    // Core valid request generator
    public static object CreateValidRequest(
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

    // Validation test cases - DisplayName validation
    public static object CreateRequestWithEmptyDisplayName() => new
    {
        DisplayName = "",
        Filterable = true,
        Searchable = true,
        IsVariant = false,
        Configuration = new Dictionary<string, object>()
    };

    public static object CreateRequestWithNullDisplayName() => new
    {
        DisplayName = (string?)null,
        Filterable = true,
        Searchable = true,
        IsVariant = false,
        Configuration = new Dictionary<string, object>()
    };

    public static object CreateRequestWithWhitespaceDisplayName() => new
    {
        DisplayName = "   ",
        Filterable = true,
        Searchable = true,
        IsVariant = false,
        Configuration = new Dictionary<string, object>()
    };

    public static object CreateRequestWithLongDisplayName() => new
    {
        DisplayName = new string('A', 101), // Exceeds 100 character limit
        Filterable = true,
        Searchable = true,
        IsVariant = false,
        Configuration = new Dictionary<string, object>()
    };

    // Boundary value testing
    public static class BoundaryTests
    {
        public static object CreateRequestWithMaximumValidDisplayNameLength() => new
        {
            DisplayName = new string('A', 100), // Exactly 100 characters
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>()
        };

        public static object CreateRequestWithMinimumValidDisplayNameLength() => new
        {
            DisplayName = "A", // Single character
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
            DisplayName = "Café Münchën Attribute™",
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>
            {
                { "unicode_value", "Ürünümüz için açıklama" },
                { "emoji", "🛍️🔥⭐" }
            }
        };

        public static object CreateRequestWithSpecialCharacters() => new
        {
            DisplayName = "Special-Chars_123!@#",
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>
            {
                { "special_chars", "!@#$%^&*()_+-=[]{}|;:,.<>?" }
            }
        };

        public static object CreateRequestWithEmptyConfiguration() => new
        {
            DisplayName = _faker.Commerce.ProductAdjective(),
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>()
        };

        public static object CreateRequestWithNullConfiguration() => new
        {
            DisplayName = _faker.Commerce.ProductAdjective(),
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = (Dictionary<string, object>?)null
        };

        public static object CreateRequestWithComplexConfiguration() => new
        {
            DisplayName = _faker.Commerce.ProductAdjective(),
            Filterable = true,
            Searchable = true,
            IsVariant = false,
            Configuration = new Dictionary<string, object>
            {
                { "string_value", "test_string" },
                { "number_value", 42 },
                { "boolean_value", true },
                { "array_value", new[] { "item1", "item2", "item3" } },
                { "nested_object", new Dictionary<string, object>
                    {
                        { "nested_string", "nested_value" },
                        { "nested_number", 123 }
                    }
                }
            }
        };
    }

    // Different property combinations
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