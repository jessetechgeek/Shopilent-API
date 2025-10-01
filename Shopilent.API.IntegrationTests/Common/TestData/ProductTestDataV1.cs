using Bogus;

namespace Shopilent.API.IntegrationTests.Common.TestData;

public static class ProductTestDataV1
{
    private static readonly Faker _faker = new();

    /// <summary>
    /// Core product creation methods for all test scenarios
    /// </summary>
    public static class Creation
    {
        /// <summary>
        /// Creates a valid product request with customizable parameters
        /// </summary>
        public static object CreateValidRequest(
            string? name = null,
            string? slug = null,
            string? description = null,
            decimal? basePrice = null,
            string? currency = null,
            string? sku = null,
            List<Guid>? categoryIds = null,
            Dictionary<string, object>? metadata = null,
            bool? isActive = null,
            List<object>? attributes = null,
            List<object>? images = null)
        {
            var productName = name ?? _faker.Commerce.ProductName();
            return new
            {
                Name = productName,
                Slug = slug ?? GenerateSlugFromName(productName),
                Description = description ?? _faker.Commerce.ProductDescription(),
                BasePrice = basePrice ?? _faker.Random.Decimal(1, 1000),
                Currency = currency ?? "USD",
                Sku = sku ?? _faker.Random.AlphaNumeric(8).ToUpper(),
                CategoryIds = categoryIds ?? new List<Guid>(),
                Metadata = metadata ?? new Dictionary<string, object>(),
                IsActive = isActive ?? true,
                Attributes = attributes ?? new List<object>(),
                Images = images ?? new List<object>()
            };
        }

        /// <summary>
        /// Creates a valid product for deletion testing with unique naming
        /// </summary>
        public static object CreateValidProductForDeletion(
            string? name = null,
            string? slug = null)
        {
            return new
            {
                Name = name ?? $"Delete Test Product {_faker.Random.AlphaNumeric(8)}",
                Slug = slug ?? $"delete-test-product-{_faker.Random.AlphaNumeric(8).ToLower()}",
                Description = _faker.Commerce.ProductDescription(),
                BasePrice = _faker.Random.Decimal(1, 1000),
                Currency = "USD",
                Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
                CategoryIds = new List<Guid>(),
                Metadata = new Dictionary<string, object>(),
                IsActive = true,
                Attributes = new List<object>(),
                Images = new List<object>()
            };
        }

        /// <summary>
        /// Creates a product for seeding test data
        /// </summary>
        public static object CreateProductForSeeding(
            string? name = null,
            string? slug = null,
            decimal? basePrice = null,
            List<Guid>? categoryIds = null)
        {
            var productName = name ?? _faker.Commerce.ProductName();
            return new
            {
                Name = productName,
                Slug = slug ?? GenerateSlugFromName(productName),
                Description = _faker.Commerce.ProductDescription(),
                BasePrice = basePrice ?? _faker.Random.Decimal(10, 500),
                Currency = "USD",
                Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
                CategoryIds = categoryIds ?? new List<Guid>(),
                Metadata = new Dictionary<string, object>(),
                IsActive = true,
                Attributes = new List<object>(),
                Images = new List<object>()
            };
        }

        /// <summary>
        /// Creates multiple products for bulk testing scenarios
        /// </summary>
        public static List<object> CreateMultipleProductsForSeeding(int count = 5)
        {
            var products = new List<object>();

            for (int i = 0; i < count; i++)
            {
                products.Add(CreateProductForSeeding(
                    name: $"Test Product {i + 1}",
                    slug: $"test-product-{i + 1}-{_faker.Random.AlphaNumeric(4).ToLower()}",
                    basePrice: _faker.Random.Decimal(10, 500)
                ));
            }

            return products;
        }

        /// <summary>
        /// Creates a product with attributes
        /// </summary>
        public static object CreateProductWithAttributes(List<Guid> attributeIds)
        {
            var productName = _faker.Commerce.ProductName();
            var attributes = attributeIds.Select(id => new
            {
                AttributeId = id,
                Value = _faker.Commerce.ProductAdjective()
            }).ToList<object>();

            return new
            {
                Name = productName,
                Slug = GenerateSlugFromName(productName),
                Description = _faker.Commerce.ProductDescription(),
                BasePrice = _faker.Random.Decimal(10, 500),
                Currency = "USD",
                Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
                CategoryIds = new List<Guid>(),
                Metadata = new Dictionary<string, object>(),
                IsActive = true,
                Attributes = attributes,
                Images = new List<object>()
            };
        }

        /// <summary>
        /// Creates a product with categories
        /// </summary>
        public static object CreateProductWithCategories(List<Guid> categoryIds)
        {
            var productName = _faker.Commerce.ProductName();
            return new
            {
                Name = productName,
                Slug = GenerateSlugFromName(productName),
                Description = _faker.Commerce.ProductDescription(),
                BasePrice = _faker.Random.Decimal(10, 500),
                Currency = "USD",
                Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
                CategoryIds = categoryIds,
                Metadata = new Dictionary<string, object>(),
                IsActive = true,
                Attributes = new List<object>(),
                Images = new List<object>()
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
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        public static object CreateRequestWithNullName() => new
        {
            Name = (string?)null,
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        public static object CreateRequestWithWhitespaceName() => new
        {
            Name = "   ",
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        public static object CreateRequestWithLongName() => new
        {
            Name = new string('A', 256), // Exceeds 255 character limit
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        // Slug validation
        public static object CreateRequestWithEmptySlug() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = "",
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        public static object CreateRequestWithNullSlug() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = (string?)null,
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        public static object CreateRequestWithInvalidSlug() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = "Invalid Slug With Spaces!",
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        public static object CreateRequestWithUppercaseSlug() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = "UPPERCASE-SLUG",
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        public static object CreateRequestWithLongSlug() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = new string('a', 256), // Exceeds 255 character limit
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        // BasePrice validation
        public static object CreateRequestWithNegativePrice() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = -10.50m,
            Currency = "USD",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        // Currency validation
        public static object CreateRequestWithEmptyCurrency() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        public static object CreateRequestWithInvalidCurrencyLength() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "US", // Should be 3 characters
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        // SKU validation
        public static object CreateRequestWithLongSku() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = new string('S', 101), // Exceeds 100 character limit
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        // Description validation
        public static object CreateRequestWithLongDescription() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = _faker.Lorem.Slug(),
            Description = new string('D', 2001), // Exceeds 2000 character limit
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        // Attribute validation
        public static object CreateRequestWithInvalidAttributeId() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>
            {
                new
                {
                    AttributeId = Guid.NewGuid(), // Non-existent attribute
                    Value = "Test Value"
                }
            },
            Images = new List<object>()
        };

        public static object CreateRequestWithEmptyAttributeId() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>
            {
                new
                {
                    AttributeId = Guid.Empty,
                    Value = "Test Value"
                }
            },
            Images = new List<object>()
        };

        // Category validation
        public static object CreateRequestWithInvalidCategoryId() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid> { Guid.NewGuid() }, // Non-existent category
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };
    }

    /// <summary>
    /// Boundary value testing for limits and edge values
    /// </summary>
    public static class BoundaryTests
    {
        public static object CreateRequestWithMaximumNameLength() => new
        {
            Name = new string('A', 255), // Exactly 255 characters
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        public static object CreateRequestWithMaximumSlugLength() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = new string('a', 255), // Exactly 255 characters
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        public static object CreateRequestWithMaximumDescriptionLength() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = _faker.Lorem.Slug(),
            Description = new string('D', 2000), // Exactly 2000 characters
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        public static object CreateRequestWithMaximumSkuLength() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = new string('S', 100), // Exactly 100 characters
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        public static object CreateRequestWithMinimumValidName() => new
        {
            Name = "A", // Single character
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        public static object CreateRequestWithMinimumValidSlug() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = "a", // Single character
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        public static object CreateRequestWithZeroPrice() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = 0m, // Minimum valid price
            Currency = "USD",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        public static object CreateRequestWithMinimumValidData() => new
        {
            Name = "A",
            Slug = "a",
            Description = (string?)null,
            BasePrice = 0m,
            Currency = "USD",
            Sku = (string?)null,
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        public static object CreateRequestWithMaximumValidData() => new
        {
            Name = new string('Z', 255),
            Slug = new string('z', 255),
            Description = new string('D', 2000),
            BasePrice = 999999.99m,
            Currency = "USD",
            Sku = new string('S', 100),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };
    }

    /// <summary>
    /// Edge cases for special scenarios
    /// </summary>
    public static class EdgeCases
    {
        public static object CreateRequestWithUnicodeCharacters() => new
        {
            Name = "Café Münchën Product™",
            Slug = "cafe-munchen-product",
            Description = "Ürünümüz için açıklama with émojis 🛍️",
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        public static object CreateRequestWithSpecialCharacters() => new
        {
            Name = "Product-With_Special.Chars@123",
            Slug = "product-with-special-chars-123",
            Description = "Description with special characters: !@#$%^&*()",
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        public static object CreateRequestWithEmptyDescription() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = _faker.Lorem.Slug(),
            Description = "",
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        public static object CreateRequestWithNullDescription() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = _faker.Lorem.Slug(),
            Description = (string?)null,
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        public static object CreateRequestWithEmptySku() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = "",
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        public static object CreateRequestWithNullSku() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = (string?)null,
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        public static object CreateRequestWithComplexMetadata() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>
            {
                { "brand", "Test Brand" },
                { "manufacturer", "Test Manufacturer" },
                { "warranty_months", 12 },
                { "tags", new[] { "tag1", "tag2", "tag3" } },
                { "featured", true },
                { "nested_data", new { key1 = "value1", key2 = 42 } }
            },
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        public static object CreateRequestWithEmptyCollections() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        public static object CreateInactiveProductRequest() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "USD",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = false,
            Attributes = new List<object>(),
            Images = new List<object>()
        };
    }

    /// <summary>
    /// Currency-specific test scenarios
    /// </summary>
    public static class CurrencyTests
    {
        public static object CreateRequestWithEUR() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "EUR",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        public static object CreateRequestWithGBP() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(1, 1000),
            Currency = "GBP",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };

        public static object CreateRequestWithJPY() => new
        {
            Name = _faker.Commerce.ProductName(),
            Slug = _faker.Lorem.Slug(),
            Description = _faker.Commerce.ProductDescription(),
            BasePrice = _faker.Random.Decimal(100, 10000),
            Currency = "JPY",
            Sku = _faker.Random.AlphaNumeric(8).ToUpper(),
            CategoryIds = new List<Guid>(),
            Metadata = new Dictionary<string, object>(),
            IsActive = true,
            Attributes = new List<object>(),
            Images = new List<object>()
        };
    }

    /// <summary>
    /// Slug pattern helpers for testing uniqueness
    /// </summary>
    public static class SlugPatterns
    {
        public static string GenerateUniqueSlug() => $"test-product-{Guid.NewGuid():N}";
        public static string GenerateSlugWithNumbers() => $"product-{_faker.Random.Number(1000, 9999)}";
        public static string GenerateSlugWithHyphens() => "test-product-with-multiple-hyphens";
    }

    /// <summary>
    /// Performance test data scenarios
    /// </summary>
    public static class Performance
    {
        public static List<object> CreateManyProducts(int count = 50)
        {
            var products = new List<object>();

            for (int i = 0; i < count; i++)
            {
                products.Add(Creation.CreateProductForSeeding(
                    name: $"Performance Test Product {i + 1:D3}",
                    slug: $"performance-test-product-{i + 1:D3}-{_faker.Random.AlphaNumeric(4).ToLower()}",
                    basePrice: _faker.Random.Decimal(10, 500)
                ));
            }

            return products;
        }
    }
}
