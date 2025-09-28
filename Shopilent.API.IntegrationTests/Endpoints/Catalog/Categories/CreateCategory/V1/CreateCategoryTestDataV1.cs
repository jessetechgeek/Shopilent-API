using Bogus;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Categories.CreateCategory.V1;

public static class CreateCategoryTestDataV1
{
    private static readonly Faker _faker = new();

    // Core valid request generator
    public static object CreateValidRequest(
        string? name = null,
        string? slug = null,
        string? description = null,
        Guid? parentId = null)
    {
        return new
        {
            Name = name ?? _faker.Commerce.Categories(1)[0],
            Slug = slug ?? _faker.Lorem.Slug(),
            Description = description ?? _faker.Lorem.Sentence(),
            ParentId = parentId
        };
    }

    // Validation test cases
    public static object CreateRequestWithEmptyName() => new
    {
        Name = "",
        Slug = _faker.Lorem.Slug(),
        Description = _faker.Lorem.Sentence(),
        ParentId = (Guid?)null
    };

    public static object CreateRequestWithNullName() => new
    {
        Name = (string)null,
        Slug = _faker.Lorem.Slug(),
        Description = _faker.Lorem.Sentence(),
        ParentId = (Guid?)null
    };

    public static object CreateRequestWithEmptySlug() => new
    {
        Name = _faker.Commerce.Categories(1)[0],
        Slug = "",
        Description = _faker.Lorem.Sentence(),
        ParentId = (Guid?)null
    };

    public static object CreateRequestWithNullSlug() => new
    {
        Name = _faker.Commerce.Categories(1)[0],
        Slug = (string)null,
        Description = _faker.Lorem.Sentence(),
        ParentId = (Guid?)null
    };

    public static object CreateRequestWithInvalidSlug() => new
    {
        Name = _faker.Commerce.Categories(1)[0],
        Slug = "Invalid Slug With Spaces!",
        Description = _faker.Lorem.Sentence(),
        ParentId = (Guid?)null
    };

    public static object CreateRequestWithUppercaseSlug() => new
    {
        Name = _faker.Commerce.Categories(1)[0],
        Slug = "UPPERCASE-SLUG",
        Description = _faker.Lorem.Sentence(),
        ParentId = (Guid?)null
    };

    // Boundary value testing
    public static class BoundaryTests
    {
        public static object CreateRequestWithMaximumNameLength() => new
        {
            Name = new string('A', 100), // Maximum valid length
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Lorem.Sentence(),
            ParentId = (Guid?)null
        };

        public static object CreateRequestWithExcessiveNameLength() => new
        {
            Name = new string('A', 101), // Exceeds maximum length
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Lorem.Sentence(),
            ParentId = (Guid?)null
        };

        public static object CreateRequestWithMaximumSlugLength() => new
        {
            Name = _faker.Commerce.Categories(1)[0],
            Slug = new string('a', 150), // Maximum valid length
            Description = _faker.Lorem.Sentence(),
            ParentId = (Guid?)null
        };

        public static object CreateRequestWithExcessiveSlugLength() => new
        {
            Name = _faker.Commerce.Categories(1)[0],
            Slug = new string('a', 151), // Exceeds maximum length
            Description = _faker.Lorem.Sentence(),
            ParentId = (Guid?)null
        };

        public static object CreateRequestWithMaximumDescriptionLength() => new
        {
            Name = _faker.Commerce.Categories(1)[0],
            Slug = _faker.Lorem.Slug(),
            Description = new string('D', 500), // Maximum valid length
            ParentId = (Guid?)null
        };

        public static object CreateRequestWithExcessiveDescriptionLength() => new
        {
            Name = _faker.Commerce.Categories(1)[0],
            Slug = _faker.Lorem.Slug(),
            Description = new string('D', 501), // Exceeds maximum length
            ParentId = (Guid?)null
        };
    }

    // Edge cases
    public static class EdgeCases
    {
        public static object CreateRequestWithUnicodeCharacters() => new
        {
            Name = "Café & Münchën Electronics™",
            Slug = "cafe-munchen-electronics",
            Description = "Ürünlerimiz için açıklama with émojis 🛍️",
            ParentId = (Guid?)null
        };

        public static object CreateRequestWithSpecialCharactersInName() => new
        {
            Name = "Electronics & Gadgets (2024)",
            Slug = "electronics-gadgets-2024",
            Description = "Category for electronics & gadgets in 2024",
            ParentId = (Guid?)null
        };

        public static object CreateRequestWithMinimalData() => new
        {
            Name = "A",
            Slug = "a",
            Description = (string)null,
            ParentId = (Guid?)null
        };

        public static object CreateRequestWithEmptyDescription() => new
        {
            Name = _faker.Commerce.Categories(1)[0],
            Slug = _faker.Lorem.Slug(),
            Description = "",
            ParentId = (Guid?)null
        };

        public static object CreateRequestWithValidParentId() => new
        {
            Name = _faker.Commerce.Categories(1)[0],
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Lorem.Sentence(),
            ParentId = Guid.NewGuid() // This will be replaced with a real parent ID in tests
        };

        public static object CreateRequestWithInvalidParentId() => new
        {
            Name = _faker.Commerce.Categories(1)[0],
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Lorem.Sentence(),
            ParentId = Guid.NewGuid() // Non-existent parent ID
        };
    }

    // Common slug patterns for testing uniqueness
    public static class SlugPatterns
    {
        public static string GenerateUniqueSlug() => $"test-category-{Guid.NewGuid():N}";
        public static string GenerateSlugWithNumbers() => $"category-{_faker.Random.Number(1000, 9999)}";
        public static string GenerateSlugWithHyphens() => "test-category-with-multiple-hyphens";
    }
}