using Bogus;
using Shopilent.Domain.Catalog.Enums;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Attributes.GetAllAttributes.V1;

public static class GetAllAttributesTestDataV1
{
    private static readonly Faker _faker = new();

    /// <summary>
    /// Creates a valid attribute creation request for seeding test data
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
    /// Test scenarios for edge cases
    /// </summary>
    public static class EdgeCases
    {
        public static object CreateAttributeWithUnicodeCharacters() => new
        {
            Name = "café_münchën_™",
            DisplayName = "Café Münchën Attribute™",
            Type = AttributeType.Text.ToString(),
            Configuration = new Dictionary<string, object>(),
            Filterable = true,
            Searchable = true,
            IsVariant = false
        };

        public static object CreateAttributeWithAllFlags() => new
        {
            Name = "full_featured_attr",
            DisplayName = "Full Featured Attribute",
            Type = AttributeType.Select.ToString(),
            Configuration = new Dictionary<string, object>
            {
                { "options", new[] { "Option 1", "Option 2", "Option 3" } },
                { "multiSelect", true }
            },
            Filterable = true,
            Searchable = true,
            IsVariant = true
        };

        public static object CreateAttributeWithMinimalFlags() => new
        {
            Name = "minimal_attr",
            DisplayName = "Minimal Attribute",
            Type = AttributeType.Boolean.ToString(),
            Configuration = new Dictionary<string, object>(),
            Filterable = false,
            Searchable = false,
            IsVariant = false
        };
    }

    /// <summary>
    /// Type-specific test data scenarios
    /// </summary>
    public static class TypeSpecific
    {
        public static List<object> CreateAttributesOfAllTypes()
        {
            var attributes = new List<object>();
            var attributeTypes = Enum.GetValues<AttributeType>();

            foreach (var type in attributeTypes)
            {
                attributes.Add(CreateAttributeForSeeding(
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

        public static object CreateTextAttribute() => CreateAttributeForSeeding(
            name: "text_search_attr",
            displayName: "Text Search Attribute",
            type: AttributeType.Text.ToString(),
            filterable: false,
            searchable: true,
            isVariant: false
        );

        public static object CreateColorAttribute() => CreateAttributeForSeeding(
            name: "color_variant_attr",
            displayName: "Color Variant Attribute",
            type: AttributeType.Color.ToString(),
            filterable: true,
            searchable: false,
            isVariant: true
        );

        public static object CreateSelectAttribute() => CreateAttributeForSeeding(
            name: "select_filter_attr",
            displayName: "Select Filter Attribute",
            type: AttributeType.Select.ToString(),
            filterable: true,
            searchable: false,
            isVariant: true
        );
    }
}