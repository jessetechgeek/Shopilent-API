using Bogus;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Categories.GetChildCategories.V1;

public static class GetChildCategoriesTestDataV1
{
    private static readonly Faker _faker = new();

    // Helper method to create a valid parent category request
    public static object CreateValidParentCategoryRequest(
        string? name = null,
        string? slug = null,
        string? description = null)
    {
        return new
        {
            Name = name ?? $"Parent {_faker.Commerce.Categories(1)[0]}",
            Slug = slug ?? _faker.Lorem.Slug(),
            Description = description ?? _faker.Lorem.Sentence(),
            ParentId = (Guid?)null
        };
    }

    // Helper method to create a valid child category request
    public static object CreateValidChildCategoryRequest(
        Guid parentId,
        string? name = null,
        string? slug = null,
        string? description = null)
    {
        return new
        {
            Name = name ?? $"Child {_faker.Commerce.Categories(1)[0]}",
            Slug = slug ?? _faker.Lorem.Slug(),
            Description = description ?? _faker.Lorem.Sentence(),
            ParentId = parentId
        };
    }

    // Multiple child categories for testing lists
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
                    Slug = $"child-category-{i + 1}-{Guid.NewGuid():N}",
                    Description = $"Description for child category {i + 1}",
                    ParentId = parentId
                });
            }
            return children;
        }

        public static List<object> CreateMixedActiveInactiveChildren(Guid parentId)
        {
            return new List<object>
            {
                new
                {
                    Name = "Active Child 1",
                    Slug = $"active-child-1-{Guid.NewGuid():N}",
                    Description = "First active child",
                    ParentId = parentId
                },
                new
                {
                    Name = "Active Child 2",
                    Slug = $"active-child-2-{Guid.NewGuid():N}",
                    Description = "Second active child",
                    ParentId = parentId
                },
                new
                {
                    Name = "Inactive Child",
                    Slug = $"inactive-child-{Guid.NewGuid():N}",
                    Description = "This child will be made inactive",
                    ParentId = parentId
                }
            };
        }
    }

    // Edge cases for testing
    public static class EdgeCases
    {
        public static Guid NonExistentParentId => Guid.NewGuid();

        public static Guid EmptyGuid => Guid.Empty;

        public static string InvalidGuidString => "not-a-valid-guid";

        public static object CreateParentWithUnicodeCharacters()
        {
            return new
            {
                Name = "Café & Münchën Parent™",
                Slug = $"cafe-munchen-parent-{Guid.NewGuid():N}",
                Description = "Parent category with unicode characters émojis 🛍️",
                ParentId = (Guid?)null
            };
        }

        public static object CreateChildWithUnicodeCharacters(Guid parentId)
        {
            return new
            {
                Name = "Niño & Niña Child™",
                Slug = $"nino-nina-child-{Guid.NewGuid():N}",
                Description = "Child category with unicode characters ñáéíóú 👶",
                ParentId = parentId
            };
        }

        // Create a deep hierarchy for testing (Parent -> Child -> Grandchild)
        public static object CreateGrandchildCategoryRequest(Guid childParentId)
        {
            return new
            {
                Name = $"Grandchild {_faker.Commerce.Categories(1)[0]}",
                Slug = $"grandchild-{_faker.Lorem.Slug()}",
                Description = _faker.Lorem.Sentence(),
                ParentId = childParentId
            };
        }
    }

    // Boundary testing scenarios
    public static class BoundaryTests
    {
        public static List<object> CreateMaximumChildren(Guid parentId, int maxCount = 50)
        {
            var children = new List<object>();
            for (int i = 0; i < maxCount; i++)
            {
                children.Add(new
                {
                    Name = $"Boundary Child {i + 1:D3}",
                    Slug = $"boundary-child-{i + 1:D3}-{Guid.NewGuid():N}",
                    Description = $"Boundary test child category {i + 1}",
                    ParentId = parentId
                });
            }
            return children;
        }

        public static object CreateParentWithMinimalData()
        {
            return new
            {
                Name = "A",
                Slug = $"a-{Guid.NewGuid():N}",
                Description = (string?)null,
                ParentId = (Guid?)null
            };
        }

        public static object CreateChildWithMinimalData(Guid parentId)
        {
            return new
            {
                Name = "B",
                Slug = $"b-{Guid.NewGuid():N}",
                Description = (string?)null,
                ParentId = parentId
            };
        }
    }

    // Helper methods for testing specific scenarios
    public static class TestScenarios
    {
        // Create a parent with no children for empty list testing
        public static object CreateParentWithNoChildren()
        {
            return new
            {
                Name = "Childless Parent",
                Slug = $"childless-parent-{Guid.NewGuid():N}",
                Description = "Parent category with no children",
                ParentId = (Guid?)null
            };
        }

        // Create hierarchical structure for testing order
        public static List<object> CreateOrderedChildren(Guid parentId)
        {
            return new List<object>
            {
                new
                {
                    Name = "Alpha Child",
                    Slug = $"alpha-child-{Guid.NewGuid():N}",
                    Description = "First alphabetically",
                    ParentId = parentId
                },
                new
                {
                    Name = "Beta Child",
                    Slug = $"beta-child-{Guid.NewGuid():N}",
                    Description = "Second alphabetically",
                    ParentId = parentId
                },
                new
                {
                    Name = "Gamma Child",
                    Slug = $"gamma-child-{Guid.NewGuid():N}",
                    Description = "Third alphabetically",
                    ParentId = parentId
                }
            };
        }
    }
}