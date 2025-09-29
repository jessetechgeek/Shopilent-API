using Bogus;
using Shopilent.Domain.Common.Models;

namespace Shopilent.API.IntegrationTests.Common.TestData;

public static class CategoryTestDataV1
{
    private static readonly Faker _faker = new();

    /// <summary>
    /// Core category creation methods for all test scenarios
    /// </summary>
    public static class Creation
    {
        /// <summary>
        /// Creates a valid category request with customizable parameters
        /// </summary>
        public static object CreateValidRequest(
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

        /// <summary>
        /// Creates a valid category for deletion testing with unique naming
        /// </summary>
        public static object CreateValidCategoryForDeletion(
            string? name = null,
            string? slug = null,
            string? description = null,
            Guid? parentId = null)
        {
            return new
            {
                Name = name ?? $"delete-test-{_faker.Random.AlphaNumeric(8).ToLower()}",
                Slug = slug ?? $"delete-test-{_faker.Random.AlphaNumeric(8).ToLower()}",
                Description = description ?? $"Delete Test {_faker.Commerce.Department()}",
                ParentId = parentId
            };
        }

        /// <summary>
        /// Creates a category for seeding test data
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
                    parentId: null
                ));
            }

            return categories;
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
                Slug = $"parent-category-{_faker.Random.AlphaNumeric(8).ToLower()}",
                ParentId = (Guid?)null
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
                Slug = $"category-products-{_faker.Random.AlphaNumeric(8).ToLower()}",
                ParentId = (Guid?)null
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
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Lorem.Sentence(),
            ParentId = (Guid?)null
        };

        public static object CreateRequestWithNullName() => new
        {
            Name = (string?)null,
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Lorem.Sentence(),
            ParentId = (Guid?)null
        };

        public static object CreateRequestWithWhitespaceName() => new
        {
            Name = "   ",
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Lorem.Sentence(),
            ParentId = (Guid?)null
        };

        public static object CreateRequestWithLongName() => new
        {
            Name = new string('A', 101), // Exceeds 100 character limit
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Lorem.Sentence(),
            ParentId = (Guid?)null
        };

        // Slug validation
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
            Slug = (string?)null,
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

        // Description validation
        public static object CreateRequestWithLongDescription() => new
        {
            Name = _faker.Commerce.Categories(1)[0],
            Slug = _faker.Lorem.Slug(),
            Description = new string('D', 501), // Exceeds 500 character limit
            ParentId = (Guid?)null
        };

        // Parent validation
        public static object CreateRequestWithInvalidParentId() => new
        {
            Name = _faker.Commerce.Categories(1)[0],
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Lorem.Sentence(),
            ParentId = Guid.NewGuid() // Non-existent parent ID
        };
    }

    /// <summary>
    /// Boundary value testing for limits and edge values
    /// </summary>
    public static class BoundaryTests
    {
        public static object CreateRequestWithMaximumNameLength() => new
        {
            Name = new string('A', 100), // Exactly 100 characters
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Lorem.Sentence(),
            ParentId = (Guid?)null
        };

        public static object CreateRequestWithMaximumSlugLength() => new
        {
            Name = _faker.Commerce.Categories(1)[0],
            Slug = new string('a', 150), // Exactly 150 characters
            Description = _faker.Lorem.Sentence(),
            ParentId = (Guid?)null
        };

        public static object CreateRequestWithMaximumDescriptionLength() => new
        {
            Name = _faker.Commerce.Categories(1)[0],
            Slug = _faker.Lorem.Slug(),
            Description = new string('B', 500), // Exactly 500 characters
            ParentId = (Guid?)null
        };

        public static object CreateRequestWithMinimumValidName() => new
        {
            Name = "A", // Single character
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Lorem.Sentence(),
            ParentId = (Guid?)null
        };

        public static object CreateRequestWithMinimumValidSlug() => new
        {
            Name = _faker.Commerce.Categories(1)[0],
            Slug = "a", // Single character
            Description = _faker.Lorem.Sentence(),
            ParentId = (Guid?)null
        };

        public static object CreateRequestWithMinimumValidData() => new
        {
            Name = "A",
            Slug = "a",
            Description = (string?)null,
            ParentId = (Guid?)null
        };

        public static object CreateRequestWithMaximumValidData() => new
        {
            Name = new string('Z', 100),
            Slug = new string('s', 150).ToLower(),
            Description = new string('d', 500),
            ParentId = (Guid?)null
        };
    }

    /// <summary>
    /// Edge cases for special scenarios
    /// </summary>
    public static class EdgeCases
    {
        public static object CreateRequestWithUnicodeCharacters() => new
        {
            Name = "Café Münchën Category™",
            Slug = "cafe-munchen-category",
            Description = "Ürünlər kateqoriyası для тестирования",
            ParentId = (Guid?)null
        };

        public static object CreateRequestWithUnicodeCharactersForCreate() => new
        {
            Name = "Café & Münchën Electronics™",
            Slug = "cafe-munchen-electronics",
            Description = "Ürünlerimiz için açıklama with émojis 🛍️",
            ParentId = (Guid?)null
        };

        public static object CreateRequestWithUnicodeCharactersForGetCategory() => new
        {
            Name = "Café & Münchën Category™",
            Slug = "cafe-munchen-category",
            Description = "Ürünler için kategori with émojis 🛍️",
            ParentId = (Guid?)null
        };

        public static object CreateRequestWithSpecialCharacters() => new
        {
            Name = "Category-With_Special.Chars@123",
            Slug = "category-with-special-chars-123",
            Description = "Description with special characters: !@#$%^&*()",
            ParentId = (Guid?)null
        };

        public static object CreateRequestWithSpecialCharactersForCreate() => new
        {
            Name = "Electronics & Gadgets (2024)",
            Slug = "electronics-gadgets-2024",
            Description = "Category for electronics & gadgets in 2024",
            ParentId = (Guid?)null
        };

        public static object CreateRequestWithSpecialCharactersInName() => new
        {
            Name = "Category-With_Special.Chars@123",
            Slug = "category-with-special-chars-123",
            Description = "Description with special characters: !@#$%^&*()",
            ParentId = (Guid?)null
        };

        public static object CreateRequestWithEmptyDescription() => new
        {
            Name = _faker.Commerce.Categories(1)[0],
            Slug = _faker.Lorem.Slug(),
            Description = "",
            ParentId = (Guid?)null
        };

        public static object CreateRequestWithNullDescription() => new
        {
            Name = _faker.Commerce.Categories(1)[0],
            Slug = _faker.Lorem.Slug(),
            Description = (string?)null,
            ParentId = (Guid?)null
        };

        public static object CreateRequestWithValidParentId() => new
        {
            Name = _faker.Commerce.Categories(1)[0],
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Lorem.Sentence(),
            ParentId = Guid.NewGuid() // This will be replaced with a real parent ID in tests
        };

        public static object CreateCategoryWithUnicodeCharacters() => new
        {
            Name = "café-münchën",
            Description = "Café Münchën Category™",
            Slug = "cafe-munchen-tm",
            ParentId = (Guid?)null
        };

        public static object CreateCategoryWithComplexMetadata() => new
        {
            Name = "complex-metadata-category",
            Description = "Category with complex metadata and special characters",
            Slug = "complex-metadata",
            ParentId = (Guid?)null
        };

        public static object CreateCategoryWithLongName() => new
        {
            Name = new string('a', 100), // Maximum valid length
            Description = "Category with maximum name length",
            Slug = "long-name-category",
            ParentId = (Guid?)null
        };

        // GetChildCategories specific edge cases
        public static Guid NonExistentParentId => Guid.NewGuid();
        public static Guid EmptyGuid => Guid.Empty;
        public static string InvalidGuidString = "invalid-guid-format";

        public static object CreateParentWithUnicodeCharacters() => new
        {
            Name = "Café Parent Category™",
            Slug = "cafe-parent-category",
            Description = "Parent category with unicode characters",
            ParentId = (Guid?)null
        };

        public static object CreateChildWithUnicodeCharacters(Guid parentId) => new
        {
            Name = "Niño & Niña Child™",
            Slug = "nino-nina-child",
            Description = "Child category with unicode characters and emojis 👶",
            ParentId = parentId
        };

        public static object CreateGrandchildCategoryRequest(Guid parentId) => new
        {
            Name = "Grandchild Category",
            Slug = "grandchild-category",
            Description = "Third level category for testing",
            ParentId = parentId
        };
    }

    /// <summary>
    /// Hierarchical category testing scenarios
    /// </summary>
    public static class Hierarchical
    {
        public static object CreateRootCategory(string? name = null) => Creation.CreateCategoryForSeeding(
            name: name ?? "root_category_test",
            slug: name?.ToLowerInvariant().Replace(" ", "-").Replace("_", "-") ?? "root-category-test",
            description: "Root Category for Testing",
            parentId: null
        );

        public static object CreateChildCategory(Guid parentId, string? name = null) => Creation.CreateCategoryForSeeding(
            name: name ?? "child_category_test",
            slug: name?.ToLowerInvariant().Replace(" ", "-").Replace("_", "-") ?? "child-category-test",
            description: "Child Category for Testing",
            parentId: parentId
        );

        public static object CreateSubChildCategory(Guid parentId, string? name = null) => Creation.CreateCategoryForSeeding(
            name: name ?? "Sub-Child Category",
            slug: name?.ToLowerInvariant().Replace(" ", "-") + "-" + _faker.Random.AlphaNumeric(4).ToLower() ?? "sub-child-category-" + _faker.Random.AlphaNumeric(4).ToLower(),
            description: "Sub-child level category",
            parentId: parentId
        );

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
    /// Category state-specific test scenarios
    /// </summary>
    public static class StatusTests
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

        public static object CreateActiveCategoryForDeletion() => new
        {
            Name = "active-category-delete",
            Description = "Active category for deletion testing",
            Slug = "active-category-delete",
            ParentId = (Guid?)null
        };

        public static object CreateInactiveCategoryForDeletion() => new
        {
            Name = "inactive-category-delete",
            Description = "Inactive category for deletion testing",
            Slug = "inactive-category-delete",
            ParentId = (Guid?)null
        };

        public static List<object> CreateMixedStatusCategories()
        {
            return new List<object>
            {
                Creation.CreateCategoryForSeeding(name: "Active Category 1", slug: "active-category-1-" + _faker.Random.AlphaNumeric(4).ToLower()),
                Creation.CreateCategoryForSeeding(name: "Inactive Category 1", slug: "inactive-category-1-" + _faker.Random.AlphaNumeric(4).ToLower()),
                Creation.CreateCategoryForSeeding(name: "Active Category 2", slug: "active-category-2-" + _faker.Random.AlphaNumeric(4).ToLower()),
                Creation.CreateCategoryForSeeding(name: "Inactive Category 2", slug: "inactive-category-2-" + _faker.Random.AlphaNumeric(4).ToLower()),
                Creation.CreateCategoryForSeeding(name: "Active Category 3", slug: "active-category-3-" + _faker.Random.AlphaNumeric(4).ToLower())
            };
        }
    }

    /// <summary>
    /// Commerce-specific category test scenarios
    /// </summary>
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

    /// <summary>
    /// Slug pattern helpers for testing uniqueness
    /// </summary>
    public static class SlugPatterns
    {
        public static string GenerateUniqueSlug() => $"test-category-{Guid.NewGuid():N}";
        public static string GenerateSlugWithNumbers() => $"category-{_faker.Random.Number(1000, 9999)}";
        public static string GenerateSlugWithHyphens() => "test-category-with-multiple-hyphens";
    }

    /// <summary>
    /// Update-specific test scenarios for UpdateCategory operations
    /// </summary>
    public static class UpdateScenarios
    {
        /// <summary>
        /// Creates a valid update request (excludes Slug as it's typically immutable)
        /// </summary>
        public static object CreateValidUpdateRequest(
            string? name = null,
            string? description = null,
            Guid? parentId = null)
        {
            return new
            {
                Name = name ?? _faker.Commerce.Categories(1)[0],
                Description = description ?? _faker.Lorem.Sentence(),
                ParentId = parentId
            };
        }

        /// <summary>
        /// Different property combinations for update testing
        /// </summary>
        public static class PropertyCombinations
        {
            public static object CreateRequestNameOnly() => new
            {
                Name = "Updated Category Name",
                Description = (string?)null,
                ParentId = (Guid?)null
            };

            public static object CreateRequestDescriptionOnly() => new
            {
                Name = (string?)null,
                Description = "Updated category description",
                ParentId = (Guid?)null
            };

            public static object CreateRequestParentOnly() => new
            {
                Name = (string?)null,
                Description = (string?)null,
                ParentId = Guid.NewGuid()
            };

            public static object CreateRequestAllFields() => new
            {
                Name = "Updated Category Name",
                Description = "Updated category description with all fields",
                ParentId = Guid.NewGuid()
            };
        }
    }

    /// <summary>
    /// Helper methods for creating related entities
    /// </summary>
    public static class RelatedEntities
    {
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
                categories.Add(Creation.CreateCategoryForSeeding(
                    name: $"Performance Test Category {i + 1:D3}",
                    slug: $"performance-test-category-{i + 1:D3}-{_faker.Random.AlphaNumeric(4).ToLower()}",
                    description: $"Performance test category number {i + 1}",
                    parentId: null
                ));
            }

            return categories;
        }
    }

    /// <summary>
    /// GetChildCategories specific test scenarios
    /// </summary>
    public static class ChildCategories
    {
        public static object CreateValidParentCategoryRequest(string? name = null, string? slug = null) => new
        {
            Name = name ?? "Parent Category",
            Slug = slug ?? $"parent-category-{_faker.Random.AlphaNumeric(4).ToLower()}",
            Description = "Parent category for child category testing",
            ParentId = (Guid?)null
        };

        public static object CreateValidChildCategoryRequest(Guid parentId, string? name = null, string? slug = null) => new
        {
            Name = name ?? "Child Category",
            Slug = slug ?? $"child-category-{_faker.Random.AlphaNumeric(4).ToLower()}",
            Description = "Child category for testing",
            ParentId = parentId
        };

        public static class MultipleChildren
        {
            public static List<object> CreateMultipleChildCategoryRequests(Guid parentId, int count = 3)
            {
                var children = new List<object>();
                for (int i = 0; i < count; i++)
                {
                    children.Add(new
                    {
                        Name = $"Child Category {i + 1}",
                        Slug = $"child-category-{i + 1}-{_faker.Random.AlphaNumeric(4).ToLower()}",
                        Description = $"Child category number {i + 1}",
                        ParentId = parentId
                    });
                }
                return children;
            }
        }

        public static class TestScenarios
        {
            public static object CreateParentWithNoChildren() => new
            {
                Name = "Parent Without Children",
                Slug = $"parent-no-children-{_faker.Random.AlphaNumeric(4).ToLower()}",
                Description = "Parent category with no child categories",
                ParentId = (Guid?)null
            };

            public static List<object> CreateOrderedChildren(Guid parentId)
            {
                return new List<object>
                {
                    new
                    {
                        Name = "Alpha Child",
                        Slug = "alpha-child",
                        Description = "Alpha child category",
                        ParentId = parentId
                    },
                    new
                    {
                        Name = "Beta Child",
                        Slug = "beta-child",
                        Description = "Beta child category",
                        ParentId = parentId
                    },
                    new
                    {
                        Name = "Gamma Child",
                        Slug = "gamma-child",
                        Description = "Gamma child category",
                        ParentId = parentId
                    }
                };
            }
        }

        public static class BoundaryTestsCh
        {
            public static object CreateParentWithMinimalData() => new
            {
                Name = "P",
                Slug = "p",
                Description = "Minimal parent",
                ParentId = (Guid?)null
            };

            public static object CreateChildWithMinimalData(Guid parentId) => new
            {
                Name = "B",
                Slug = "b",
                Description = (string?)null,
                ParentId = parentId
            };

            public static List<object> CreateMaximumChildren(Guid parentId, int count = 20)
            {
                var children = new List<object>();
                for (int i = 0; i < count; i++)
                {
                    children.Add(new
                    {
                        Name = $"Max Child {i + 1:D2}",
                        Slug = $"max-child-{i + 1:D2}",
                        Description = $"Maximum test child {i + 1}",
                        ParentId = parentId
                    });
                }
                return children;
            }
        }
    }

    /// <summary>
    /// DataTable-specific test scenarios (simple cases only - complex ones use DataTableTestDataFactory)
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
                    new() { Data = "slug", Name = "slug", Searchable = true, Orderable = true },
                    new() { Data = "description", Name = "description", Searchable = true, Orderable = true },
                    new() { Data = "parentName", Name = "parentName", Searchable = true, Orderable = true }
                },
                Order = new List<DataTableOrder>
                {
                    new() { Column = 0, Dir = "asc" }
                }
            };
        }
    }
}