using Bogus;

namespace Shopilent.API.IntegrationTests.Common.TestData;

public static class CartTestDataV1
{
    private static readonly Faker _faker = new();

    /// <summary>
    /// Core cart item creation methods for all test scenarios
    /// </summary>
    public static class Creation
    {
        /// <summary>
        /// Creates a valid AddItemToCart request with customizable parameters
        /// </summary>
        public static object CreateValidRequest(
            Guid? cartId = null,
            Guid? productId = null,
            Guid? variantId = null,
            int? quantity = null)
        {
            return new
            {
                CartId = cartId,
                ProductId = productId ?? Guid.NewGuid(), // Will be replaced with actual product in tests
                VariantId = variantId,
                Quantity = quantity ?? 1
            };
        }

        /// <summary>
        /// Creates a request with multiple items quantity
        /// </summary>
        public static object CreateRequestWithMultipleQuantity(
            Guid? cartId = null,
            Guid? productId = null,
            Guid? variantId = null,
            int quantity = 5)
        {
            return new
            {
                CartId = cartId,
                ProductId = productId ?? Guid.NewGuid(),
                VariantId = variantId,
                Quantity = quantity
            };
        }

        /// <summary>
        /// Creates a request for adding an item with variant
        /// </summary>
        public static object CreateRequestWithVariant(
            Guid? cartId = null,
            Guid? productId = null,
            Guid? variantId = null,
            int? quantity = null)
        {
            return new
            {
                CartId = cartId,
                ProductId = productId ?? Guid.NewGuid(),
                VariantId = variantId ?? Guid.NewGuid(), // Will be replaced with actual variant in tests
                Quantity = quantity ?? 1
            };
        }

        /// <summary>
        /// Creates a request without specifying cart ID (for cart creation test)
        /// </summary>
        public static object CreateRequestWithoutCartId(
            Guid? productId = null,
            Guid? variantId = null,
            int? quantity = null)
        {
            return new
            {
                CartId = (Guid?)null,
                ProductId = productId ?? Guid.NewGuid(),
                VariantId = variantId,
                Quantity = quantity ?? 1
            };
        }
    }

    /// <summary>
    /// Validation test cases for various field validations
    /// </summary>
    public static class Validation
    {
        // ProductId validation
        public static object CreateRequestWithEmptyProductId(Guid? cartId = null) => new
        {
            CartId = cartId,
            ProductId = Guid.Empty,
            VariantId = (Guid?)null,
            Quantity = 1
        };

        public static object CreateRequestWithoutProductId(Guid? cartId = null) => new
        {
            CartId = cartId,
            VariantId = (Guid?)null,
            Quantity = 1
        };

        // Quantity validation
        public static object CreateRequestWithZeroQuantity(
            Guid? cartId = null,
            Guid? productId = null) => new
        {
            CartId = cartId,
            ProductId = productId ?? Guid.NewGuid(),
            VariantId = (Guid?)null,
            Quantity = 0
        };

        public static object CreateRequestWithNegativeQuantity(
            Guid? cartId = null,
            Guid? productId = null) => new
        {
            CartId = cartId,
            ProductId = productId ?? Guid.NewGuid(),
            VariantId = (Guid?)null,
            Quantity = -5
        };

        public static object CreateRequestWithExcessiveQuantity(
            Guid? cartId = null,
            Guid? productId = null) => new
        {
            CartId = cartId,
            ProductId = productId ?? Guid.NewGuid(),
            VariantId = (Guid?)null,
            Quantity = 101 // Exceeds 100 limit
        };

        // Non-existent entity validation
        public static object CreateRequestWithNonExistentProductId(Guid? cartId = null) => new
        {
            CartId = cartId,
            ProductId = Guid.NewGuid(), // Random non-existent product
            VariantId = (Guid?)null,
            Quantity = 1
        };

        public static object CreateRequestWithNonExistentVariantId(
            Guid? cartId = null,
            Guid? productId = null) => new
        {
            CartId = cartId,
            ProductId = productId ?? Guid.NewGuid(),
            VariantId = Guid.NewGuid(), // Random non-existent variant
            Quantity = 1
        };

        public static object CreateRequestWithNonExistentCartId(Guid? productId = null) => new
        {
            CartId = Guid.NewGuid(), // Random non-existent cart
            ProductId = productId ?? Guid.NewGuid(),
            VariantId = (Guid?)null,
            Quantity = 1
        };
    }

    /// <summary>
    /// Boundary value testing for limits and edge values
    /// </summary>
    public static class BoundaryTests
    {
        public static object CreateRequestWithMinimumValidQuantity(
            Guid? cartId = null,
            Guid? productId = null) => new
        {
            CartId = cartId,
            ProductId = productId ?? Guid.NewGuid(),
            VariantId = (Guid?)null,
            Quantity = 1 // Minimum valid quantity
        };

        public static object CreateRequestWithMaximumValidQuantity(
            Guid? cartId = null,
            Guid? productId = null) => new
        {
            CartId = cartId,
            ProductId = productId ?? Guid.NewGuid(),
            VariantId = (Guid?)null,
            Quantity = 100 // Maximum valid quantity
        };

        public static object CreateRequestAtQuantityBoundary(
            Guid? cartId = null,
            Guid? productId = null,
            int quantity = 99) => new
        {
            CartId = cartId,
            ProductId = productId ?? Guid.NewGuid(),
            VariantId = (Guid?)null,
            Quantity = quantity // Just below maximum
        };
    }

    /// <summary>
    /// Edge cases for special scenarios
    /// </summary>
    public static class EdgeCases
    {
        /// <summary>
        /// Creates a request for adding same product multiple times
        /// </summary>
        public static object CreateRequestForDuplicateProduct(
            Guid cartId,
            Guid productId,
            int quantity = 1) => new
        {
            CartId = cartId,
            ProductId = productId,
            VariantId = (Guid?)null,
            Quantity = quantity
        };

        /// <summary>
        /// Creates a request for adding same variant multiple times
        /// </summary>
        public static object CreateRequestForDuplicateVariant(
            Guid cartId,
            Guid productId,
            Guid variantId,
            int quantity = 1) => new
        {
            CartId = cartId,
            ProductId = productId,
            VariantId = variantId,
            Quantity = quantity
        };

        /// <summary>
        /// Creates a request with null variant (for product without variants)
        /// </summary>
        public static object CreateRequestWithNullVariant(
            Guid? cartId = null,
            Guid? productId = null) => new
        {
            CartId = cartId,
            ProductId = productId ?? Guid.NewGuid(),
            VariantId = (Guid?)null,
            Quantity = 1
        };

        /// <summary>
        /// Creates multiple requests for bulk cart operations
        /// </summary>
        public static List<object> CreateMultipleAddRequests(
            Guid cartId,
            List<Guid> productIds,
            int quantityPerProduct = 1)
        {
            var requests = new List<object>();

            foreach (var productId in productIds)
            {
                requests.Add(new
                {
                    CartId = cartId,
                    ProductId = productId,
                    VariantId = (Guid?)null,
                    Quantity = quantityPerProduct
                });
            }

            return requests;
        }
    }

    /// <summary>
    /// Performance test data scenarios
    /// </summary>
    public static class Performance
    {
        public static List<object> CreateConcurrentAddRequests(
            int count = 10,
            Guid? cartId = null)
        {
            var requests = new List<object>();

            for (int i = 0; i < count; i++)
            {
                requests.Add(new
                {
                    CartId = cartId,
                    ProductId = Guid.NewGuid(), // Will be replaced with actual products in tests
                    VariantId = (Guid?)null,
                    Quantity = _faker.Random.Int(1, 5)
                });
            }

            return requests;
        }
    }

    /// <summary>
    /// Cart state test scenarios
    /// </summary>
    public static class CartState
    {
        /// <summary>
        /// Creates request for adding to empty cart
        /// </summary>
        public static object CreateRequestForEmptyCart(
            Guid cartId,
            Guid productId,
            int quantity = 1) => new
        {
            CartId = cartId,
            ProductId = productId,
            VariantId = (Guid?)null,
            Quantity = quantity
        };

        /// <summary>
        /// Creates request for adding to cart with existing items
        /// </summary>
        public static object CreateRequestForExistingCart(
            Guid cartId,
            Guid productId,
            Guid? variantId = null,
            int quantity = 1) => new
        {
            CartId = cartId,
            ProductId = productId,
            VariantId = variantId,
            Quantity = quantity
        };

        /// <summary>
        /// Creates request for anonymous cart (no cart ID, no authentication)
        /// </summary>
        public static object CreateRequestForAnonymousCart(
            Guid productId,
            int quantity = 1) => new
        {
            CartId = (Guid?)null,
            ProductId = productId,
            VariantId = (Guid?)null,
            Quantity = quantity
        };

        /// <summary>
        /// Creates request for authenticated user without existing cart
        /// </summary>
        public static object CreateRequestForNewUserCart(
            Guid productId,
            Guid? variantId = null,
            int quantity = 1) => new
        {
            CartId = (Guid?)null,
            ProductId = productId,
            VariantId = variantId,
            Quantity = quantity
        };
    }

    /// <summary>
    /// Quantity update scenarios
    /// </summary>
    public static class QuantityScenarios
    {
        public static List<int> GetValidQuantities() => new() { 1, 2, 5, 10, 25, 50, 100 };

        public static List<int> GetInvalidQuantities() => new() { -100, -1, 0, 101, 200, 1000 };

        public static object CreateRequestWithQuantity(
            Guid? cartId,
            Guid productId,
            int quantity) => new
        {
            CartId = cartId,
            ProductId = productId,
            VariantId = (Guid?)null,
            Quantity = quantity
        };
    }

    /// <summary>
    /// Clear cart scenarios
    /// </summary>
    public static class ClearCart
    {
        /// <summary>
        /// Creates a valid clear cart request with cart ID
        /// </summary>
        public static object CreateValidRequestWithCartId(Guid cartId) => new
        {
            CartId = cartId
        };

        /// <summary>
        /// Creates a request without cart ID (for authenticated users)
        /// </summary>
        public static object CreateRequestWithoutCartId() => new
        {
            CartId = (Guid?)null
        };

        /// <summary>
        /// Creates a request with empty cart ID (invalid)
        /// </summary>
        public static object CreateRequestWithEmptyCartId() => new
        {
            CartId = Guid.Empty
        };

        /// <summary>
        /// Creates a request with non-existent cart ID
        /// </summary>
        public static object CreateRequestWithNonExistentCartId() => new
        {
            CartId = Guid.NewGuid() // Random non-existent cart
        };
    }

    /// <summary>
    /// Update cart item quantity scenarios
    /// </summary>
    public static class UpdateQuantity
    {
        /// <summary>
        /// Creates a valid update quantity request
        /// </summary>
        public static object CreateValidRequest(int? quantity = null) => new
        {
            Quantity = quantity ?? 5
        };

        /// <summary>
        /// Creates a request with specific quantity value
        /// </summary>
        public static object CreateRequestWithQuantity(int quantity) => new
        {
            Quantity = quantity
        };

        /// <summary>
        /// Creates a request with zero quantity (invalid)
        /// </summary>
        public static object CreateRequestWithZeroQuantity() => new
        {
            Quantity = 0
        };

        /// <summary>
        /// Creates a request with negative quantity (invalid)
        /// </summary>
        public static object CreateRequestWithNegativeQuantity() => new
        {
            Quantity = -5
        };

        /// <summary>
        /// Creates a request with excessive quantity (invalid)
        /// </summary>
        public static object CreateRequestWithExcessiveQuantity() => new
        {
            Quantity = 1000 // Exceeds 999 limit
        };

        /// <summary>
        /// Creates a request with minimum valid quantity
        /// </summary>
        public static object CreateRequestWithMinimumValidQuantity() => new
        {
            Quantity = 1
        };

        /// <summary>
        /// Creates a request with maximum valid quantity
        /// </summary>
        public static object CreateRequestWithMaximumValidQuantity() => new
        {
            Quantity = 999
        };

        /// <summary>
        /// Creates a request just below maximum valid quantity
        /// </summary>
        public static object CreateRequestWithNearMaximumQuantity() => new
        {
            Quantity = 998
        };

        /// <summary>
        /// Creates a request with quantity at boundary (just above maximum)
        /// </summary>
        public static object CreateRequestWithJustAboveMaximumQuantity() => new
        {
            Quantity = 1000
        };

        /// <summary>
        /// Gets a list of valid quantities for theory testing
        /// </summary>
        public static List<int> GetValidQuantitiesForUpdate() => new() { 1, 2, 5, 10, 50, 100, 500, 999 };

        /// <summary>
        /// Gets a list of invalid quantities for theory testing
        /// </summary>
        public static List<int> GetInvalidQuantitiesForUpdate() => new() { -100, -1, 0, 1000, 2000 };
    }
}
