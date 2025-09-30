using System.Text;
using System.Text.Json;
using Bogus;
using Shopilent.API.Common.Models;

namespace Shopilent.API.IntegrationTests.Common.TestData;

public static class UniversalSearchTestDataV1
{
    private static readonly Faker _faker = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Core search request creation methods for all test scenarios
    /// </summary>
    public static class Creation
    {
        /// <summary>
        /// Creates a valid search request with customizable filters
        /// </summary>
        public static object CreateValidRequest(
            string? searchQuery = null,
            string[]? categorySlugs = null,
            Dictionary<string, string[]>? attributeFilters = null,
            decimal? priceMin = null,
            decimal? priceMax = null,
            bool? inStockOnly = null,
            bool? activeOnly = null,
            int? pageNumber = null,
            int? pageSize = null,
            string? sortBy = null,
            bool? sortDescending = null)
        {
            var filters = new ProductFilters
            {
                SearchQuery = searchQuery ?? "",
                CategorySlugs = categorySlugs ?? Array.Empty<string>(),
                AttributeFilters = attributeFilters ?? new Dictionary<string, string[]>(),
                PriceMin = priceMin,
                PriceMax = priceMax,
                InStockOnly = inStockOnly ?? false,
                ActiveOnly = activeOnly ?? true,
                PageNumber = pageNumber ?? 1,
                PageSize = pageSize ?? 20,
                SortBy = sortBy ?? "relevance",
                SortDescending = sortDescending ?? false
            };

            return new
            {
                FiltersBase64 = EncodeFilters(filters)
            };
        }

        /// <summary>
        /// Creates a basic search request with just a search query
        /// </summary>
        public static object CreateBasicSearchRequest(string searchQuery = "laptop")
        {
            return CreateValidRequest(searchQuery: searchQuery);
        }

        /// <summary>
        /// Creates a search request with category filters
        /// </summary>
        public static object CreateCategoryFilteredRequest(params string[] categorySlugs)
        {
            return CreateValidRequest(
                searchQuery: _faker.Commerce.ProductName(),
                categorySlugs: categorySlugs.Length > 0 ? categorySlugs : new[] { "electronics", "computers" }
            );
        }

        /// <summary>
        /// Creates a search request with price range filters
        /// </summary>
        public static object CreatePriceFilteredRequest(
            decimal? priceMin = null,
            decimal? priceMax = null,
            string? searchQuery = null)
        {
            return CreateValidRequest(
                searchQuery: searchQuery ?? _faker.Commerce.ProductName(),
                priceMin: priceMin ?? 100.00m,
                priceMax: priceMax ?? 500.00m
            );
        }

        /// <summary>
        /// Creates a search request with attribute filters
        /// </summary>
        public static object CreateAttributeFilteredRequest(
            Dictionary<string, string[]>? attributeFilters = null,
            string? searchQuery = null)
        {
            var filters = attributeFilters ?? new Dictionary<string, string[]>
            {
                { "color", new[] { "red", "blue" } },
                { "size", new[] { "medium", "large" } }
            };

            return CreateValidRequest(
                searchQuery: searchQuery ?? _faker.Commerce.ProductName(),
                attributeFilters: filters
            );
        }

        /// <summary>
        /// Creates a paginated search request
        /// </summary>
        public static object CreatePaginatedRequest(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchQuery = null)
        {
            return CreateValidRequest(
                searchQuery: searchQuery ?? _faker.Commerce.ProductName(),
                pageNumber: pageNumber,
                pageSize: pageSize
            );
        }

        /// <summary>
        /// Creates a sorted search request
        /// </summary>
        public static object CreateSortedRequest(
            string sortBy = "name",
            bool sortDescending = false,
            string? searchQuery = null)
        {
            return CreateValidRequest(
                searchQuery: searchQuery ?? _faker.Commerce.ProductName(),
                sortBy: sortBy,
                sortDescending: sortDescending
            );
        }

        /// <summary>
        /// Creates a comprehensive search request with all filters
        /// </summary>
        public static object CreateComprehensiveRequest()
        {
            return CreateValidRequest(
                searchQuery: "premium laptop computer",
                categorySlugs: new[] { "electronics", "computers" },
                attributeFilters: new Dictionary<string, string[]>
                {
                    { "brand", new[] { "apple", "dell" } },
                    { "color", new[] { "silver", "black" } },
                    { "screen_size", new[] { "13", "15" } }
                },
                priceMin: 500.00m,
                priceMax: 2000.00m,
                inStockOnly: true,
                activeOnly: true,
                pageNumber: 1,
                pageSize: 20,
                sortBy: "price",
                sortDescending: false
            );
        }
    }

    /// <summary>
    /// Validation test cases for various input validations
    /// </summary>
    public static class Validation
    {
        /// <summary>
        /// Creates a request with empty FiltersBase64
        /// </summary>
        public static object CreateRequestWithEmptyFiltersBase64() => new
        {
            FiltersBase64 = ""
        };

        /// <summary>
        /// Creates a request with null FiltersBase64
        /// </summary>
        public static object CreateRequestWithNullFiltersBase64() => new
        {
            FiltersBase64 = (string?)null
        };

        /// <summary>
        /// Creates a request with whitespace-only FiltersBase64
        /// </summary>
        public static object CreateRequestWithWhitespaceFiltersBase64() => new
        {
            FiltersBase64 = "   "
        };

        /// <summary>
        /// Creates a request with invalid base64 string
        /// </summary>
        public static object CreateRequestWithInvalidBase64() => new
        {
            FiltersBase64 = "Invalid_Base64_String_With_Invalid_Characters!"
        };

        /// <summary>
        /// Creates a request with valid base64 but invalid JSON
        /// </summary>
        public static object CreateRequestWithInvalidJson()
        {
            var invalidJson = "{ invalid json structure }";
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(invalidJson));
            return new
            {
                FiltersBase64 = base64
            };
        }

        /// <summary>
        /// Creates a request with malformed JSON structure
        /// </summary>
        public static object CreateRequestWithMalformedJson()
        {
            var malformedJson = @"{ ""searchQuery"": ""test"", ""pageNumber"": ""not_a_number"" }";
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(malformedJson));
            return new
            {
                FiltersBase64 = base64
            };
        }

        /// <summary>
        /// Creates a request with invalid category slug format
        /// </summary>
        public static object CreateRequestWithInvalidCategorySlugs()
        {
            var filters = new ProductFilters
            {
                SearchQuery = "test",
                CategorySlugs = new[] { "Invalid Category Slug!", "UPPERCASE_SLUG", "slug with spaces" },
                PageNumber = 1,
                PageSize = 10
            };

            return new
            {
                FiltersBase64 = EncodeFilters(filters)
            };
        }

        /// <summary>
        /// Creates a request with invalid price range (min > max)
        /// </summary>
        public static object CreateRequestWithInvalidPriceRange()
        {
            var filters = new ProductFilters
            {
                SearchQuery = "test",
                PriceMin = 1000.00m,
                PriceMax = 100.00m, // Max less than min
                PageNumber = 1,
                PageSize = 10
            };

            return new
            {
                FiltersBase64 = EncodeFilters(filters)
            };
        }

        /// <summary>
        /// Creates a request with negative price values
        /// </summary>
        public static object CreateRequestWithNegativePrices()
        {
            var filters = new ProductFilters
            {
                SearchQuery = "test",
                PriceMin = -100.00m,
                PriceMax = -50.00m,
                PageNumber = 1,
                PageSize = 10
            };

            return new
            {
                FiltersBase64 = EncodeFilters(filters)
            };
        }

        /// <summary>
        /// Creates a request with invalid page number (zero or negative)
        /// </summary>
        public static object CreateRequestWithInvalidPageNumber()
        {
            var filters = new ProductFilters
            {
                SearchQuery = "test",
                PageNumber = 0, // Invalid page number
                PageSize = 10
            };

            return new
            {
                FiltersBase64 = EncodeFilters(filters)
            };
        }

        /// <summary>
        /// Creates a request with invalid page size (zero or negative)
        /// </summary>
        public static object CreateRequestWithInvalidPageSize()
        {
            var filters = new ProductFilters
            {
                SearchQuery = "test",
                PageNumber = 1,
                PageSize = 0 // Invalid page size
            };

            return new
            {
                FiltersBase64 = EncodeFilters(filters)
            };
        }
    }

    /// <summary>
    /// Boundary value testing for limits and edge values
    /// </summary>
    public static class BoundaryTests
    {
        /// <summary>
        /// Creates a request with minimum valid page values
        /// </summary>
        public static object CreateRequestWithMinimumPageValues()
        {
            return Creation.CreateValidRequest(
                pageNumber: 1,
                pageSize: 1
            );
        }

        /// <summary>
        /// Creates a request with maximum valid page size
        /// </summary>
        public static object CreateRequestWithMaximumPageSize()
        {
            return Creation.CreateValidRequest(
                pageNumber: 1,
                pageSize: 100
            );
        }

        /// <summary>
        /// Creates a request with very large page number
        /// </summary>
        public static object CreateRequestWithLargePageNumber()
        {
            return Creation.CreateValidRequest(
                pageNumber: 9999,
                pageSize: 10
            );
        }

        /// <summary>
        /// Creates a request with minimum price values
        /// </summary>
        public static object CreateRequestWithMinimumPrices()
        {
            return Creation.CreatePriceFilteredRequest(
                priceMin: 0.01m,
                priceMax: 0.02m
            );
        }

        /// <summary>
        /// Creates a request with maximum price values
        /// </summary>
        public static object CreateRequestWithMaximumPrices()
        {
            return Creation.CreatePriceFilteredRequest(
                priceMin: 999999.99m,
                priceMax: 999999.99m
            );
        }

        /// <summary>
        /// Creates a request with maximum number of category slugs
        /// </summary>
        public static object CreateRequestWithMaximumCategorySlugs()
        {
            var categorySlugs = Enumerable.Range(1, 20)
                .Select(i => $"category-{i}")
                .ToArray();

            return Creation.CreateCategoryFilteredRequest(categorySlugs);
        }

        /// <summary>
        /// Creates a request with maximum number of attribute filters
        /// </summary>
        public static object CreateRequestWithMaximumAttributeFilters()
        {
            var attributeFilters = new Dictionary<string, string[]>();
            for (int i = 1; i <= 10; i++)
            {
                attributeFilters[$"attribute_{i}"] = new[] { $"value_{i}_1", $"value_{i}_2" };
            }

            return Creation.CreateAttributeFilteredRequest(attributeFilters);
        }
    }

    /// <summary>
    /// Edge cases for special scenarios
    /// </summary>
    public static class EdgeCases
    {
        /// <summary>
        /// Creates a request with empty search query
        /// </summary>
        public static object CreateRequestWithEmptySearchQuery()
        {
            return Creation.CreateValidRequest(searchQuery: "");
        }

        /// <summary>
        /// Creates a request with unicode characters in search query
        /// </summary>
        public static object CreateRequestWithUnicodeSearchQuery()
        {
            return Creation.CreateValidRequest(
                searchQuery: "café münchën laptop™ 测试"
            );
        }

        /// <summary>
        /// Creates a request with special characters in search query
        /// </summary>
        public static object CreateRequestWithSpecialCharactersSearchQuery()
        {
            return Creation.CreateValidRequest(
                searchQuery: "laptop @2024 50% off! #premium"
            );
        }

        /// <summary>
        /// Creates a request with very long search query
        /// </summary>
        public static object CreateRequestWithLongSearchQuery()
        {
            var longQuery = string.Join(" ", Enumerable.Repeat("laptop computer premium", 50));
            return Creation.CreateValidRequest(searchQuery: longQuery);
        }

        /// <summary>
        /// Creates a request with unicode characters in category slugs
        /// </summary>
        public static object CreateRequestWithUnicodeCategorySlugs()
        {
            return Creation.CreateCategoryFilteredRequest(
                "café-münchën", "niño-category", "测试-category"
            );
        }

        /// <summary>
        /// Creates a request with empty attribute filter values
        /// </summary>
        public static object CreateRequestWithEmptyAttributeFilterValues()
        {
            var attributeFilters = new Dictionary<string, string[]>
            {
                { "color", Array.Empty<string>() },
                { "size", new[] { "medium" } }
            };

            return Creation.CreateAttributeFilteredRequest(attributeFilters);
        }

        /// <summary>
        /// Creates a request with only stock filter (no search query)
        /// </summary>
        public static object CreateRequestWithStockFilterOnly()
        {
            return Creation.CreateValidRequest(
                searchQuery: "",
                inStockOnly: true
            );
        }

        /// <summary>
        /// Creates a request with only active filter (no search query)
        /// </summary>
        public static object CreateRequestWithActiveFilterOnly()
        {
            return Creation.CreateValidRequest(
                searchQuery: "",
                activeOnly: true
            );
        }

        /// <summary>
        /// Creates a request with all boolean filters set to false
        /// </summary>
        public static object CreateRequestWithAllBooleanFiltersFalse()
        {
            return Creation.CreateValidRequest(
                searchQuery: "test",
                inStockOnly: false,
                activeOnly: false,
                sortDescending: false
            );
        }

        /// <summary>
        /// Creates a request that should return no results
        /// </summary>
        public static object CreateRequestWithNoExpectedResults()
        {
            return Creation.CreateValidRequest(
                searchQuery: "nonexistent_product_xyz_123",
                categorySlugs: new[] { "nonexistent-category" },
                priceMin: 999999.99m,
                priceMax: 999999.99m
            );
        }
    }

    /// <summary>
    /// Sort-specific test scenarios
    /// </summary>
    public static class SortingTests
    {
        public static object CreateRequestWithRelevanceSort()
        {
            return Creation.CreateSortedRequest("relevance", false);
        }

        public static object CreateRequestWithNameSortAscending()
        {
            return Creation.CreateSortedRequest("name", false);
        }

        public static object CreateRequestWithNameSortDescending()
        {
            return Creation.CreateSortedRequest("name", true);
        }

        public static object CreateRequestWithPriceSortAscending()
        {
            return Creation.CreateSortedRequest("price", false);
        }

        public static object CreateRequestWithPriceSortDescending()
        {
            return Creation.CreateSortedRequest("price", true);
        }

        public static object CreateRequestWithDateSortAscending()
        {
            return Creation.CreateSortedRequest("createdAt", false);
        }

        public static object CreateRequestWithDateSortDescending()
        {
            return Creation.CreateSortedRequest("createdAt", true);
        }

        public static object CreateRequestWithInvalidSortBy()
        {
            return Creation.CreateSortedRequest("invalid_sort_field", false);
        }
    }

    /// <summary>
    /// Performance and load testing scenarios
    /// </summary>
    public static class PerformanceTests
    {
        /// <summary>
        /// Creates multiple concurrent search requests
        /// </summary>
        public static List<object> CreateMultipleConcurrentRequests(int count = 10)
        {
            var requests = new List<object>();
            for (int i = 0; i < count; i++)
            {
                requests.Add(Creation.CreateValidRequest(
                    searchQuery: $"concurrent_search_{i}",
                    pageNumber: i % 5 + 1,
                    pageSize: 20
                ));
            }
            return requests;
        }

        /// <summary>
        /// Creates requests with complex filter combinations
        /// </summary>
        public static object CreateComplexFilterRequest()
        {
            var attributeFilters = new Dictionary<string, string[]>();
            for (int i = 1; i <= 5; i++)
            {
                attributeFilters[$"attr_{i}"] = Enumerable.Range(1, 3).Select(j => $"value_{i}_{j}").ToArray();
            }

            return Creation.CreateValidRequest(
                searchQuery: "complex search with multiple filters",
                categorySlugs: new[] { "electronics", "computers", "accessories", "mobile", "tablets" },
                attributeFilters: attributeFilters,
                priceMin: 100.00m,
                priceMax: 1000.00m,
                inStockOnly: true,
                activeOnly: true,
                pageNumber: 1,
                pageSize: 50,
                sortBy: "price",
                sortDescending: true
            );
        }
    }

    /// <summary>
    /// Helper method to encode ProductFilters to base64
    /// </summary>
    private static string EncodeFilters(ProductFilters filters)
    {
        var jsonString = JsonSerializer.Serialize(filters, JsonOptions);
        var jsonBytes = Encoding.UTF8.GetBytes(jsonString);
        return Convert.ToBase64String(jsonBytes);
    }

    /// <summary>
    /// Helper method to create valid category slugs for testing
    /// </summary>
    public static string[] CreateValidCategorySlugs(int count = 3)
    {
        return Enumerable.Range(1, count)
            .Select(i => $"category-{i}-{_faker.Random.AlphaNumeric(4).ToLower()}")
            .ToArray();
    }

    /// <summary>
    /// Helper method to create valid attribute filters for testing
    /// </summary>
    public static Dictionary<string, string[]> CreateValidAttributeFilters()
    {
        return new Dictionary<string, string[]>
        {
            { "color", new[] { "red", "blue", "green" } },
            { "size", new[] { "small", "medium", "large" } },
            { "brand", new[] { "apple", "samsung", "google" } },
            { "material", new[] { "metal", "plastic", "glass" } }
        };
    }
}