using Bogus;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Categories.GetAllCategories.V1;

public static class GetAllCategoriesTestDataV1
{
    private static readonly Faker _faker = new();

    /// <summary>
    /// Creates a valid category creation request for seeding test data
    /// </summary>
    public static object CreateCategoryForSeeding(
        string? name = null,
        string? slug = null,
        string? description = null,
        Guid? parentId = null)
    {
        var categoryName = name ?? _faker.Commerce.Categories(1)[0];
        return new
        {
            Name = categoryName,
            Slug = slug ?? GenerateSlugFromName(categoryName),
            Description = description ?? _faker.Lorem.Sentence(),
            ParentId = parentId
        };
    }

    private static string GenerateSlugFromName(string name)
    {
        return name.ToLowerInvariant()
                  .Replace(" ", "-")
                  .Replace("&", "and")
                  .Replace("'", "")
                  .Replace(".", "")
                  .Replace(",", "")
                  + "-" + _faker.Random.AlphaNumeric(4).ToLower();
    }

    /// <summary>
    /// Creates multiple categories for bulk testing scenarios
    /// </summary>
    public static List<object> CreateMultipleCategoriesForSeeding(int count = 5)
    {
        var categories = new List<object>();

        for (int i = 0; i < count; i++)
        {
            categories.Add(CreateCategoryForSeeding(
                name: $"Test Category {i + 1}",
                slug: $"test-category-{i + 1}-{_faker.Random.AlphaNumeric(4).ToLower()}",
                description: $"Description for test category {i + 1}",
                parentId: null // Root categories for simplicity
            ));
        }

        return categories;
    }

    /// <summary>
    /// Creates hierarchical categories for testing parent-child relationships
    /// </summary>
    public static class Hierarchical
    {
        public static object CreateRootCategory(string? name = null) => CreateCategoryForSeeding(
            name: name ?? "Root Category",
            slug: name?.ToLowerInvariant().Replace(" ", "-") + "-" + _faker.Random.AlphaNumeric(4).ToLower() ?? "root-category-" + _faker.Random.AlphaNumeric(4).ToLower(),
            description: "Root level category",
            parentId: null
        );

        public static object CreateChildCategory(Guid parentId, string? name = null) => CreateCategoryForSeeding(
            name: name ?? "Child Category",
            slug: name?.ToLowerInvariant().Replace(" ", "-") + "-" + _faker.Random.AlphaNumeric(4).ToLower() ?? "child-category-" + _faker.Random.AlphaNumeric(4).ToLower(),
            description: "Child level category",
            parentId: parentId
        );

        public static object CreateSubChildCategory(Guid parentId, string? name = null) => CreateCategoryForSeeding(
            name: name ?? "Sub-Child Category",
            slug: name?.ToLowerInvariant().Replace(" ", "-") + "-" + _faker.Random.AlphaNumeric(4).ToLower() ?? "sub-child-category-" + _faker.Random.AlphaNumeric(4).ToLower(),
            description: "Sub-child level category",
            parentId: parentId
        );
    }

    /// <summary>
    /// Test scenarios for edge cases
    /// </summary>
    public static class EdgeCases
    {
        public static object CreateCategoryWithUnicodeCharacters() => new
        {
            Name = "Café Münchën Category™",
            Slug = "cafe-munchen-category-" + _faker.Random.AlphaNumeric(4).ToLower(),
            Description = "Ürünlər kateqoriyası для тестирования",
            ParentId = (Guid?)null
        };

        public static object CreateCategoryWithLongName() => new
        {
            Name = new string('A', 100), // Maximum valid length for testing
            Slug = "long-name-category-" + _faker.Random.AlphaNumeric(4).ToLower(),
            Description = "Category with very long name",
            ParentId = (Guid?)null
        };

        public static object CreateCategoryWithMinimalData() => new
        {
            Name = "Min",
            Slug = "min-" + _faker.Random.AlphaNumeric(4).ToLower(),
            Description = "A",
            ParentId = (Guid?)null
        };
    }

    /// <summary>
    /// Status-specific test data scenarios
    /// </summary>
    public static class StatusSpecific
    {
        public static List<object> CreateMixedStatusCategories()
        {
            return new List<object>
            {
                CreateCategoryForSeeding(name: "Active Category 1", slug: "active-category-1-" + _faker.Random.AlphaNumeric(4).ToLower()),
                CreateCategoryForSeeding(name: "Inactive Category 1", slug: "inactive-category-1-" + _faker.Random.AlphaNumeric(4).ToLower()),
                CreateCategoryForSeeding(name: "Active Category 2", slug: "active-category-2-" + _faker.Random.AlphaNumeric(4).ToLower()),
                CreateCategoryForSeeding(name: "Inactive Category 2", slug: "inactive-category-2-" + _faker.Random.AlphaNumeric(4).ToLower()),
                CreateCategoryForSeeding(name: "Active Category 3", slug: "active-category-3-" + _faker.Random.AlphaNumeric(4).ToLower())
            };
        }

        public static object CreateActiveCategory() => CreateCategoryForSeeding(
            name: "Active Status Category",
            slug: "active-status-category-" + _faker.Random.AlphaNumeric(4).ToLower(),
            description: "This category is active"
        );
    }

    /// <summary>
    /// Performance test data scenarios
    /// </summary>
    public static class Performance
    {
        public static List<object> CreateManyCategories(int count = 50)
        {
            var categories = new List<object>();

            for (int i = 0; i < count; i++)
            {
                categories.Add(CreateCategoryForSeeding(
                    name: $"Performance Test Category {i + 1:D3}",
                    slug: $"performance-test-category-{i + 1:D3}-{_faker.Random.AlphaNumeric(4).ToLower()}",
                    description: $"Performance test category number {i + 1}",
                    parentId: null
                ));
            }

            return categories;
        }
    }
}