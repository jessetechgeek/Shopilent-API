using Bogus;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Categories.GetCategory.V1;

public static class GetCategoryTestDataV1
{
    private static readonly Faker _faker = new();

    // Core valid request generator for creating test categories
    public static object CreateValidCategoryRequest(
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

    // Category hierarchy tests
    public static class HierarchyTests
    {
        public static object CreateRootCategoryRequest() => new
        {
            Name = "root_category_test",
            Slug = "root-category-test",
            Description = "Root Category for Testing",
            ParentId = (Guid?)null
        };

        public static object CreateChildCategoryRequest(Guid parentId) => new
        {
            Name = "child_category_test",
            Slug = "child-category-test",
            Description = "Child Category for Testing",
            ParentId = parentId
        };

        public static object CreateSubChildCategoryRequest(Guid parentId) => new
        {
            Name = "subchild_category_test",
            Slug = "subchild-category-test",
            Description = "Sub-Child Category for Testing",
            ParentId = parentId
        };
    }

    // Different category states for testing
    public static class CategoryStates
    {
        public static object CreateActiveCategoryRequest() => new
        {
            Name = "active_category_test",
            Slug = "active-category-test",
            Description = "Active Category for Testing",
            ParentId = (Guid?)null
        };

        public static object CreateInactiveCategoryRequest() => new
        {
            Name = "inactive_category_test",
            Slug = "inactive-category-test",
            Description = "Inactive Category for Testing",
            ParentId = (Guid?)null
        };
    }

    // Edge cases for testing
    public static class EdgeCases
    {
        public static object CreateCategoryWithUnicodeChars() => new
        {
            Name = "Café & Münchën Category™",
            Slug = "cafe-munchen-category",
            Description = "Ürünler için kategori with émojis 🛍️",
            ParentId = (Guid?)null
        };

        public static object CreateCategoryWithSpecialChars() => new
        {
            Name = "Category-With_Special.Chars@123",
            Slug = "category-with-special-chars-123",
            Description = "Description with special characters: !@#$%^&*()",
            ParentId = (Guid?)null
        };

        public static object CreateCategoryWithLongName() => new
        {
            Name = new string('A', 100), // Long category name
            Slug = "long-name-category",
            Description = "Category with very long name for testing",
            ParentId = (Guid?)null
        };

        public static object CreateCategoryWithLongDescription() => new
        {
            Name = "long_description_test",
            Slug = "long-description-test",
            Description = new string('B', 500), // Long description
            ParentId = (Guid?)null
        };
    }

    // Boundary tests
    public static class BoundaryTests
    {
        public static object CreateCategoryWithMinimumName() => new
        {
            Name = "A", // Minimum valid name length
            Slug = "a",
            Description = "Minimum name length test",
            ParentId = (Guid?)null
        };

        public static object CreateCategoryWithMaximumName() => new
        {
            Name = new string('Z', 100), // Maximum valid name length
            Slug = "maximum-name-test",
            Description = "Maximum name length test",
            ParentId = (Guid?)null
        };
    }

    // Categories for commerce testing
    public static class CommerceCategories
    {
        public static object CreateElectronicsCategoryRequest() => new
        {
            Name = "Electronics",
            Slug = "electronics",
            Description = "Electronic devices and gadgets",
            ParentId = (Guid?)null
        };

        public static object CreateClothingCategoryRequest() => new
        {
            Name = "Clothing",
            Slug = "clothing",
            Description = "Fashion and apparel items",
            ParentId = (Guid?)null
        };

        public static object CreateBooksCategoryRequest() => new
        {
            Name = "Books",
            Slug = "books",
            Description = "Books and educational materials",
            ParentId = (Guid?)null
        };
    }
}