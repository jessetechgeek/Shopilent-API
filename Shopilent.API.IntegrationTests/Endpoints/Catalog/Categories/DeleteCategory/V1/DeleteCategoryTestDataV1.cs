using Bogus;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Categories.DeleteCategory.V1;

public static class DeleteCategoryTestDataV1
{
    private static readonly Faker _faker = new();

    /// <summary>
    /// Creates a valid category for deletion testing
    /// </summary>
    public static object CreateValidCategoryForDeletion(
        string? name = null,
        string? description = null,
        string? slug = null,
        Guid? parentId = null)
    {
        return new
        {
            Name = name ?? $"delete-test-{_faker.Random.AlphaNumeric(8).ToLower()}",
            Description = description ?? $"Delete Test {_faker.Commerce.Department()}",
            Slug = slug ?? $"delete-test-{_faker.Random.AlphaNumeric(8).ToLower()}",
            ParentId = parentId
        };
    }

    /// <summary>
    /// Creates a category that has child categories (for conflict testing)
    /// </summary>
    public static object CreateCategoryWithChildren()
    {
        return new
        {
            Name = "parent-category",
            Description = "Parent Category with Children",
            Slug = $"parent-category-{_faker.Random.AlphaNumeric(8).ToLower()}"
        };
    }

    /// <summary>
    /// Creates a category that has associated products (for conflict testing)
    /// </summary>
    public static object CreateCategoryWithProducts()
    {
        return new
        {
            Name = "category-with-products",
            Description = "Category with Associated Products",
            Slug = $"category-products-{_faker.Random.AlphaNumeric(8).ToLower()}"
        };
    }

    /// <summary>
    /// Edge cases for testing various scenarios
    /// </summary>
    public static class EdgeCases
    {
        public static object CreateCategoryWithUnicodeCharacters()
        {
            return new
            {
                Name = "café-münchën",
                Description = "Café Münchën Category™",
                Slug = "cafe-munchen-tm"
            };
        }

        public static object CreateCategoryWithLongName()
        {
            return new
            {
                Name = new string('a', 100), // Maximum valid length
                Description = "Category with maximum name length",
                Slug = "long-name-category"
            };
        }

        public static object CreateCategoryWithComplexMetadata()
        {
            return new
            {
                Name = "complex-metadata-category",
                Description = "Category with complex metadata and special characters",
                Slug = "complex-metadata"
            };
        }

        public static object CreateInactiveCategoryForDeletion()
        {
            return new
            {
                Name = "inactive-category-delete",
                Description = "Inactive category for deletion testing",
                Slug = "inactive-category-delete"
            };
        }
    }

    /// <summary>
    /// Helper methods for creating related entities
    /// </summary>
    public static class RelatedEntities
    {
        public static object CreateChildCategory(Guid parentId, string? name = null)
        {
            return new
            {
                Name = name ?? $"child-category-{_faker.Random.AlphaNumeric(8).ToLower()}",
                Description = "Child category for testing",
                Slug = $"child-{_faker.Random.AlphaNumeric(8).ToLower()}",
                ParentId = parentId
            };
        }

        public static object CreateProductInCategory(Guid categoryId, string? name = null)
        {
            var productName = name ?? $"Product in Category {_faker.Commerce.ProductName()}";
            return new
            {
                Name = productName,
                Slug = productName.ToLower().Replace(" ", "-").Replace("'", ""),
                Description = _faker.Commerce.ProductDescription(),
                BasePrice = _faker.Random.Decimal(1, 1000),
                Currency = "USD",
                Sku = $"SKU-{_faker.Random.AlphaNumeric(8).ToUpper()}",
                CategoryIds = new[] { categoryId },
                IsActive = true,
                Metadata = new Dictionary<string, object>(),
                Attributes = new List<object>(),
                Images = new List<object>()
            };
        }

        public static object CreateSubcategoryHierarchy(Guid rootCategoryId)
        {
            return new[]
            {
                new
                {
                    Name = "level-1-subcategory",
                    Description = "Level 1 subcategory",
                    Slug = "level-1-subcategory",
                    ParentId = rootCategoryId
                }
            };
        }
    }

    /// <summary>
    /// Boundary value test cases
    /// </summary>
    public static class BoundaryTests
    {
        public static object CreateCategoryWithMinimumValidData()
        {
            return new
            {
                Name = "a", // Minimum valid length
                Description = "Min",
                Slug = "a"
            };
        }

        public static object CreateCategoryWithMaximumValidData()
        {
            return new
            {
                Name = new string('z', 100), // Maximum valid length (100 chars)
                Description = new string('d', 500), // Maximum description length (500 chars)
                Slug = new string('s', 150).ToLower() // Maximum slug length (150 chars), must be lowercase
            };
        }
    }

    /// <summary>
    /// Status-specific test cases
    /// </summary>
    public static class StatusTests
    {
        public static object CreateActiveCategoryForDeletion()
        {
            return new
            {
                Name = "active-category-delete",
                Description = "Active category for deletion testing",
                Slug = "active-category-delete"
            };
        }

        public static object CreateInactiveCategoryForDeletion()
        {
            return new
            {
                Name = "inactive-category-delete",
                Description = "Inactive category for deletion testing",
                Slug = "inactive-category-delete"
            };
        }
    }
}