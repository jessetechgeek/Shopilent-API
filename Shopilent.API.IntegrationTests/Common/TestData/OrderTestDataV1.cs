using Bogus;

namespace Shopilent.API.IntegrationTests.Common.TestData;

/// <summary>
/// Unified test data factory for CreateOrderFromCart endpoint operations.
/// Centralizes all order creation test data generation following established patterns.
/// </summary>
public static class OrderTestDataV1
{
    private static readonly Faker _faker = new();

    /// <summary>
    /// Core order creation methods for all test scenarios
    /// </summary>
    public static class Creation
    {
        /// <summary>
        /// Creates a valid CreateOrderFromCart request with customizable parameters
        /// </summary>
        public static object CreateValidRequest(
            Guid? cartId = null,
            Guid? shippingAddressId = null,
            Guid? billingAddressId = null,
            string? shippingMethod = null,
            Dictionary<string, object>? metadata = null)
        {
            return new
            {
                CartId = cartId,
                ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
                BillingAddressId = billingAddressId,
                ShippingMethod = shippingMethod,
                Metadata = metadata
            };
        }

        /// <summary>
        /// Creates a request without billing address (will use shipping address)
        /// </summary>
        public static object CreateRequestWithoutBillingAddress(
            Guid? cartId = null,
            Guid? shippingAddressId = null,
            string? shippingMethod = null)
        {
            return new
            {
                CartId = cartId,
                ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
                BillingAddressId = (Guid?)null,
                ShippingMethod = shippingMethod,
                Metadata = (Dictionary<string, object>?)null
            };
        }

        /// <summary>
        /// Creates a request without cart ID (will use user's default cart)
        /// </summary>
        public static object CreateRequestWithoutCartId(
            Guid? shippingAddressId = null,
            Guid? billingAddressId = null,
            string? shippingMethod = null)
        {
            return new
            {
                CartId = (Guid?)null,
                ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
                BillingAddressId = billingAddressId,
                ShippingMethod = shippingMethod,
                Metadata = (Dictionary<string, object>?)null
            };
        }

        /// <summary>
        /// Creates a request with standard shipping method
        /// </summary>
        public static object CreateRequestWithStandardShipping(
            Guid? cartId = null,
            Guid? shippingAddressId = null,
            Guid? billingAddressId = null)
        {
            return new
            {
                CartId = cartId,
                ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
                BillingAddressId = billingAddressId,
                ShippingMethod = "Standard",
                Metadata = (Dictionary<string, object>?)null
            };
        }

        /// <summary>
        /// Creates a request with express shipping method
        /// </summary>
        public static object CreateRequestWithExpressShipping(
            Guid? cartId = null,
            Guid? shippingAddressId = null,
            Guid? billingAddressId = null)
        {
            return new
            {
                CartId = cartId,
                ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
                BillingAddressId = billingAddressId,
                ShippingMethod = "Express",
                Metadata = (Dictionary<string, object>?)null
            };
        }

        /// <summary>
        /// Creates a request with overnight shipping method
        /// </summary>
        public static object CreateRequestWithOvernightShipping(
            Guid? cartId = null,
            Guid? shippingAddressId = null,
            Guid? billingAddressId = null)
        {
            return new
            {
                CartId = cartId,
                ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
                BillingAddressId = billingAddressId,
                ShippingMethod = "Overnight",
                Metadata = (Dictionary<string, object>?)null
            };
        }

        /// <summary>
        /// Creates a request with simple metadata
        /// </summary>
        public static object CreateRequestWithMetadata(
            Guid? cartId = null,
            Guid? shippingAddressId = null,
            string? metadataKey = null,
            object? metadataValue = null)
        {
            return new
            {
                CartId = cartId,
                ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
                BillingAddressId = (Guid?)null,
                ShippingMethod = (string?)null,
                Metadata = new Dictionary<string, object>
                {
                    { metadataKey ?? "test_key", metadataValue ?? "test_value" }
                }
            };
        }
    }

    /// <summary>
    /// Validation test cases for various field validations
    /// </summary>
    public static class Validation
    {
        // ShippingAddressId validation (required)
        public static object CreateRequestWithEmptyShippingAddressId() => new
        {
            CartId = (Guid?)null,
            ShippingAddressId = Guid.Empty,
            BillingAddressId = (Guid?)null,
            ShippingMethod = (string?)null,
            Metadata = (Dictionary<string, object>?)null
        };

        public static object CreateRequestWithNonExistentShippingAddressId() => new
        {
            CartId = (Guid?)null,
            ShippingAddressId = Guid.NewGuid(), // Random non-existent address
            BillingAddressId = (Guid?)null,
            ShippingMethod = (string?)null,
            Metadata = (Dictionary<string, object>?)null
        };

        // BillingAddressId validation (optional)
        public static object CreateRequestWithNonExistentBillingAddressId(
            Guid? shippingAddressId = null) => new
        {
            CartId = (Guid?)null,
            ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
            BillingAddressId = Guid.NewGuid(), // Random non-existent address
            ShippingMethod = (string?)null,
            Metadata = (Dictionary<string, object>?)null
        };

        public static object CreateRequestWithEmptyBillingAddressId(
            Guid? shippingAddressId = null) => new
        {
            CartId = (Guid?)null,
            ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
            BillingAddressId = Guid.Empty,
            ShippingMethod = (string?)null,
            Metadata = (Dictionary<string, object>?)null
        };

        // CartId validation (optional)
        public static object CreateRequestWithNonExistentCartId(
            Guid? shippingAddressId = null) => new
        {
            CartId = Guid.NewGuid(), // Random non-existent cart
            ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
            BillingAddressId = (Guid?)null,
            ShippingMethod = (string?)null,
            Metadata = (Dictionary<string, object>?)null
        };

        public static object CreateRequestWithEmptyCartId(
            Guid? shippingAddressId = null) => new
        {
            CartId = Guid.Empty,
            ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
            BillingAddressId = (Guid?)null,
            ShippingMethod = (string?)null,
            Metadata = (Dictionary<string, object>?)null
        };

        // ShippingMethod validation (optional)
        public static object CreateRequestWithLongShippingMethod(
            Guid? shippingAddressId = null) => new
        {
            CartId = (Guid?)null,
            ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
            BillingAddressId = (Guid?)null,
            ShippingMethod = new string('A', 101), // Exceeds 100 character limit
            Metadata = (Dictionary<string, object>?)null
        };

        public static object CreateRequestWithEmptyShippingMethod(
            Guid? shippingAddressId = null) => new
        {
            CartId = (Guid?)null,
            ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
            BillingAddressId = (Guid?)null,
            ShippingMethod = "",
            Metadata = (Dictionary<string, object>?)null
        };

        public static object CreateRequestWithWhitespaceShippingMethod(
            Guid? shippingAddressId = null) => new
        {
            CartId = (Guid?)null,
            ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
            BillingAddressId = (Guid?)null,
            ShippingMethod = "   ",
            Metadata = (Dictionary<string, object>?)null
        };
    }

    /// <summary>
    /// Boundary value testing for limits and edge values
    /// </summary>
    public static class BoundaryTests
    {
        public static object CreateRequestWithMaximumLengthShippingMethod(
            Guid? shippingAddressId = null) => new
        {
            CartId = (Guid?)null,
            ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
            BillingAddressId = (Guid?)null,
            ShippingMethod = new string('A', 100), // Exactly 100 characters
            Metadata = (Dictionary<string, object>?)null
        };

        public static object CreateRequestWithMinimumValidShippingMethod(
            Guid? shippingAddressId = null) => new
        {
            CartId = (Guid?)null,
            ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
            BillingAddressId = (Guid?)null,
            ShippingMethod = "A", // Single character
            Metadata = (Dictionary<string, object>?)null
        };
    }

    /// <summary>
    /// Edge cases for special scenarios
    /// </summary>
    public static class EdgeCases
    {
        public static object CreateRequestWithComplexMetadata(
            Guid? cartId = null,
            Guid? shippingAddressId = null) => new
        {
            CartId = cartId,
            ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
            BillingAddressId = (Guid?)null,
            ShippingMethod = "Standard",
            Metadata = new Dictionary<string, object>
            {
                { "gift_message", "Happy Birthday! 🎉" },
                { "gift_wrapping", true },
                { "priority_shipping", "urgent" },
                { "special_instructions", "Please leave at the front door" },
                { "delivery_window", new { start = "09:00", end = "17:00" } },
                { "contact_phone", "+1234567890" },
                { "alternative_contact", new { name = "John Doe", phone = "+0987654321" } },
                { "package_insurance", true },
                { "signature_required", false },
                { "tracking_notifications", new[] { "email", "sms" } },
                { "numeric_value", 42 },
                { "decimal_value", 123.45 },
                { "array_value", new[] { "item1", "item2", "item3" } },
                { "nested_object", new { level1 = new { level2 = "deep_value" } } },
                { "unicode_text", "Café Münchën™ 日本 🛍️" }
            }
        };

        public static object CreateRequestWithEmptyMetadata(
            Guid? cartId = null,
            Guid? shippingAddressId = null) => new
        {
            CartId = cartId,
            ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
            BillingAddressId = (Guid?)null,
            ShippingMethod = (string?)null,
            Metadata = new Dictionary<string, object>()
        };

        public static object CreateRequestWithNullMetadata(
            Guid? cartId = null,
            Guid? shippingAddressId = null) => new
        {
            CartId = cartId,
            ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
            BillingAddressId = (Guid?)null,
            ShippingMethod = (string?)null,
            Metadata = (Dictionary<string, object>?)null
        };

        public static object CreateRequestWithUnicodeShippingMethod(
            Guid? shippingAddressId = null) => new
        {
            CartId = (Guid?)null,
            ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
            BillingAddressId = (Guid?)null,
            ShippingMethod = "Café Express™ 配送",
            Metadata = (Dictionary<string, object>?)null
        };

        public static object CreateRequestWithSpecialCharactersInShippingMethod(
            Guid? shippingAddressId = null) => new
        {
            CartId = (Guid?)null,
            ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
            BillingAddressId = (Guid?)null,
            ShippingMethod = "Express-24/7 (Overnight) @ $29.99!",
            Metadata = (Dictionary<string, object>?)null
        };

        public static object CreateRequestWithNumericShippingMethod(
            Guid? shippingAddressId = null) => new
        {
            CartId = (Guid?)null,
            ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
            BillingAddressId = (Guid?)null,
            ShippingMethod = "12345",
            Metadata = (Dictionary<string, object>?)null
        };

        public static object CreateRequestWithWhitespaceInShippingMethod(
            Guid? shippingAddressId = null) => new
        {
            CartId = (Guid?)null,
            ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
            BillingAddressId = (Guid?)null,
            ShippingMethod = "  Express Shipping  ",
            Metadata = (Dictionary<string, object>?)null
        };

        /// <summary>
        /// Creates metadata with various data types
        /// </summary>
        public static object CreateRequestWithMixedTypeMetadata(
            Guid? shippingAddressId = null) => new
        {
            CartId = (Guid?)null,
            ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
            BillingAddressId = (Guid?)null,
            ShippingMethod = "Standard",
            Metadata = new Dictionary<string, object>
            {
                { "string_value", "test" },
                { "int_value", 123 },
                { "long_value", 9876543210L },
                { "double_value", 123.456 },
                { "decimal_value", 789.012m },
                { "bool_value", true },
                { "date_value", DateTime.UtcNow.ToString("o") },
                { "null_value", null! },
                { "array_value", new[] { 1, 2, 3 } },
                { "object_value", new { nested = "value" } }
            }
        };

        /// <summary>
        /// Creates metadata with long string values
        /// </summary>
        public static object CreateRequestWithLongMetadataValues(
            Guid? shippingAddressId = null) => new
        {
            CartId = (Guid?)null,
            ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
            BillingAddressId = (Guid?)null,
            ShippingMethod = "Standard",
            Metadata = new Dictionary<string, object>
            {
                { "long_message", new string('A', 5000) },
                { "long_instructions", new string('B', 10000) }
            }
        };

        /// <summary>
        /// Creates metadata with many keys
        /// </summary>
        public static object CreateRequestWithManyMetadataKeys(
            Guid? shippingAddressId = null)
        {
            var metadata = new Dictionary<string, object>();
            for (int i = 0; i < 50; i++)
            {
                metadata[$"key_{i}"] = $"value_{i}";
            }

            return new
            {
                CartId = (Guid?)null,
                ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
                BillingAddressId = (Guid?)null,
                ShippingMethod = "Standard",
                Metadata = metadata
            };
        }
    }

    /// <summary>
    /// Shipping method specific test scenarios
    /// </summary>
    public static class ShippingMethodScenarios
    {
        public static string[] GetValidShippingMethods() => new[]
        {
            "Standard",
            "Express",
            "Overnight",
            "Same-Day",
            "International",
            "Economy",
            "Priority"
        };

        public static object CreateRequestWithShippingMethod(
            Guid? shippingAddressId,
            string shippingMethod) => new
        {
            CartId = (Guid?)null,
            ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
            BillingAddressId = (Guid?)null,
            ShippingMethod = shippingMethod,
            Metadata = (Dictionary<string, object>?)null
        };

        /// <summary>
        /// Creates multiple requests with different shipping methods for testing
        /// </summary>
        public static List<object> CreateRequestsWithAllShippingMethods(Guid shippingAddressId)
        {
            return GetValidShippingMethods()
                .Select(method => CreateRequestWithShippingMethod(shippingAddressId, method))
                .ToList();
        }
    }

    /// <summary>
    /// Address combination test scenarios
    /// </summary>
    public static class AddressScenarios
    {
        /// <summary>
        /// Creates request with same address for shipping and billing
        /// </summary>
        public static object CreateRequestWithSameAddress(
            Guid addressId) => new
        {
            CartId = (Guid?)null,
            ShippingAddressId = addressId,
            BillingAddressId = addressId,
            ShippingMethod = (string?)null,
            Metadata = (Dictionary<string, object>?)null
        };

        /// <summary>
        /// Creates request with different addresses for shipping and billing
        /// </summary>
        public static object CreateRequestWithDifferentAddresses(
            Guid shippingAddressId,
            Guid billingAddressId) => new
        {
            CartId = (Guid?)null,
            ShippingAddressId = shippingAddressId,
            BillingAddressId = billingAddressId,
            ShippingMethod = (string?)null,
            Metadata = (Dictionary<string, object>?)null
        };

        /// <summary>
        /// Creates request with shipping address only (billing will default to shipping)
        /// </summary>
        public static object CreateRequestWithShippingAddressOnly(
            Guid shippingAddressId) => new
        {
            CartId = (Guid?)null,
            ShippingAddressId = shippingAddressId,
            BillingAddressId = (Guid?)null,
            ShippingMethod = (string?)null,
            Metadata = (Dictionary<string, object>?)null
        };
    }

    /// <summary>
    /// Security test scenarios
    /// </summary>
    public static class SecurityTests
    {
        public static object CreateRequestWithSqlInjectionInShippingMethod(
            Guid? shippingAddressId = null) => new
        {
            CartId = (Guid?)null,
            ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
            BillingAddressId = (Guid?)null,
            ShippingMethod = "'; DROP TABLE Orders; --",
            Metadata = (Dictionary<string, object>?)null
        };

        public static object CreateRequestWithXssInMetadata(
            Guid? shippingAddressId = null) => new
        {
            CartId = (Guid?)null,
            ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
            BillingAddressId = (Guid?)null,
            ShippingMethod = "Standard",
            Metadata = new Dictionary<string, object>
            {
                { "gift_message", "<script>alert('xss')</script>" },
                { "special_instructions", "<img src=x onerror=alert('xss')>" },
                { "recipient_name", "javascript:alert('xss')" }
            }
        };

        public static object CreateRequestWithCommandInjectionInMetadata(
            Guid? shippingAddressId = null) => new
        {
            CartId = (Guid?)null,
            ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
            BillingAddressId = (Guid?)null,
            ShippingMethod = "Standard",
            Metadata = new Dictionary<string, object>
            {
                { "instructions", "; rm -rf /" },
                { "contact", "| cat /etc/passwd" },
                { "notes", "&& whoami" }
            }
        };

        public static object CreateRequestWithNullByteInShippingMethod(
            Guid? shippingAddressId = null) => new
        {
            CartId = (Guid?)null,
            ShippingAddressId = shippingAddressId ?? Guid.NewGuid(),
            BillingAddressId = (Guid?)null,
            ShippingMethod = "Express\0Admin",
            Metadata = (Dictionary<string, object>?)null
        };
    }

    /// <summary>
    /// Cart state test scenarios
    /// </summary>
    public static class CartState
    {
        /// <summary>
        /// Creates request for order from empty cart (should fail)
        /// </summary>
        public static object CreateRequestForEmptyCart(
            Guid cartId,
            Guid shippingAddressId) => new
        {
            CartId = cartId,
            ShippingAddressId = shippingAddressId,
            BillingAddressId = (Guid?)null,
            ShippingMethod = (string?)null,
            Metadata = (Dictionary<string, object>?)null
        };

        /// <summary>
        /// Creates request for order from cart with single item
        /// </summary>
        public static object CreateRequestForSingleItemCart(
            Guid? cartId,
            Guid shippingAddressId) => new
        {
            CartId = cartId,
            ShippingAddressId = shippingAddressId,
            BillingAddressId = (Guid?)null,
            ShippingMethod = "Standard",
            Metadata = (Dictionary<string, object>?)null
        };

        /// <summary>
        /// Creates request for order from cart with multiple items
        /// </summary>
        public static object CreateRequestForMultipleItemCart(
            Guid? cartId,
            Guid shippingAddressId) => new
        {
            CartId = cartId,
            ShippingAddressId = shippingAddressId,
            BillingAddressId = (Guid?)null,
            ShippingMethod = "Standard",
            Metadata = (Dictionary<string, object>?)null
        };
    }

    /// <summary>
    /// Order shipping test scenarios for MarkOrderAsShipped endpoint
    /// </summary>
    public static class Shipping
    {
        /// <summary>
        /// Creates a valid MarkOrderAsShipped request with customizable tracking number
        /// </summary>
        public static object CreateValidRequest(string? trackingNumber = null)
        {
            return new
            {
                TrackingNumber = trackingNumber ?? _faker.Random.AlphaNumeric(20)
            };
        }

        /// <summary>
        /// Creates a request without tracking number (optional field)
        /// </summary>
        public static object CreateRequestWithoutTrackingNumber() => new
        {
            TrackingNumber = (string?)null
        };

        /// <summary>
        /// Creates a request with empty tracking number
        /// </summary>
        public static object CreateRequestWithEmptyTrackingNumber() => new
        {
            TrackingNumber = ""
        };

        /// <summary>
        /// Creates a request with whitespace tracking number
        /// </summary>
        public static object CreateRequestWithWhitespaceTrackingNumber() => new
        {
            TrackingNumber = "   "
        };

        /// <summary>
        /// Creates a request with maximum length tracking number (100 characters)
        /// </summary>
        public static object CreateRequestWithMaximumLengthTrackingNumber() => new
        {
            TrackingNumber = new string('A', 100)
        };

        /// <summary>
        /// Creates a request with tracking number exceeding maximum length (101 characters)
        /// </summary>
        public static object CreateRequestWithTooLongTrackingNumber() => new
        {
            TrackingNumber = new string('A', 101)
        };

        /// <summary>
        /// Creates a request with a valid UPS tracking number format
        /// </summary>
        public static object CreateRequestWithUpsTrackingNumber() => new
        {
            TrackingNumber = "1Z999AA10123456784"
        };

        /// <summary>
        /// Creates a request with a valid FedEx tracking number format
        /// </summary>
        public static object CreateRequestWithFedExTrackingNumber() => new
        {
            TrackingNumber = "123456789012"
        };

        /// <summary>
        /// Creates a request with a valid USPS tracking number format
        /// </summary>
        public static object CreateRequestWithUspsTrackingNumber() => new
        {
            TrackingNumber = "9400111899560942344901"
        };

        /// <summary>
        /// Creates a request with a valid DHL tracking number format
        /// </summary>
        public static object CreateRequestWithDhlTrackingNumber() => new
        {
            TrackingNumber = "1234567890"
        };

        /// <summary>
        /// Creates a request with unicode characters in tracking number
        /// </summary>
        public static object CreateRequestWithUnicodeTrackingNumber() => new
        {
            TrackingNumber = "TRACK-™-日本-123456"
        };

        /// <summary>
        /// Creates a request with special characters in tracking number
        /// </summary>
        public static object CreateRequestWithSpecialCharactersTrackingNumber() => new
        {
            TrackingNumber = "TRACK-2024-#001-@US"
        };

        /// <summary>
        /// Creates a request with numeric-only tracking number
        /// </summary>
        public static object CreateRequestWithNumericTrackingNumber() => new
        {
            TrackingNumber = "1234567890123456"
        };

        /// <summary>
        /// Creates a request with alphanumeric tracking number
        /// </summary>
        public static object CreateRequestWithAlphanumericTrackingNumber() => new
        {
            TrackingNumber = "ABC123XYZ456"
        };

        /// <summary>
        /// Creates a request with short tracking number (single character)
        /// </summary>
        public static object CreateRequestWithShortTrackingNumber() => new
        {
            TrackingNumber = "A"
        };

        /// <summary>
        /// Predefined tracking number formats for common carriers
        /// </summary>
        public static class CommonFormats
        {
            public const string UPS = "1Z999AA10123456784";
            public const string FedEx = "123456789012";
            public const string USPS = "9400111899560942344901";
            public const string DHL = "1234567890";
            public const string AmazonLogistics = "TBA123456789000";
            public const string CanadaPost = "1234567890123456";
        }
    }

    /// <summary>
    /// Order status update test scenarios for UpdateOrderStatus endpoint
    /// </summary>
    public static class StatusUpdate
    {
        /// <summary>
        /// Creates a valid UpdateOrderStatus request with customizable parameters
        /// </summary>
        public static object CreateValidRequest(
            Shopilent.Domain.Sales.Enums.OrderStatus status,
            string? reason = null)
        {
            return new
            {
                Status = status,
                Reason = reason
            };
        }

        /// <summary>
        /// Creates a request to update to Processing status
        /// </summary>
        public static object CreateRequestToProcessing(string? reason = null) => new
        {
            Status = Shopilent.Domain.Sales.Enums.OrderStatus.Processing,
            Reason = reason
        };

        /// <summary>
        /// Creates a request to update to Shipped status
        /// </summary>
        public static object CreateRequestToShipped(string? reason = null) => new
        {
            Status = Shopilent.Domain.Sales.Enums.OrderStatus.Shipped,
            Reason = reason
        };

        /// <summary>
        /// Creates a request to update to Delivered status
        /// </summary>
        public static object CreateRequestToDelivered(string? reason = null) => new
        {
            Status = Shopilent.Domain.Sales.Enums.OrderStatus.Delivered,
            Reason = reason
        };

        /// <summary>
        /// Creates a request to update to Cancelled status
        /// </summary>
        public static object CreateRequestToCancelled(string? reason = null) => new
        {
            Status = Shopilent.Domain.Sales.Enums.OrderStatus.Cancelled,
            Reason = reason
        };

        /// <summary>
        /// Creates a request to update to Pending status (invalid transition)
        /// </summary>
        public static object CreateRequestToPending(string? reason = null) => new
        {
            Status = Shopilent.Domain.Sales.Enums.OrderStatus.Pending,
            Reason = reason
        };

        /// <summary>
        /// Creates a request with detailed reason
        /// </summary>
        public static object CreateRequestWithDetailedReason(
            Shopilent.Domain.Sales.Enums.OrderStatus status) => new
        {
            Status = status,
            Reason = "Order status updated after thorough review and verification of all processing requirements and business rules."
        };

        /// <summary>
        /// Creates a request without reason (optional field)
        /// </summary>
        public static object CreateRequestWithoutReason(
            Shopilent.Domain.Sales.Enums.OrderStatus status) => new
        {
            Status = status,
            Reason = (string?)null
        };

        /// <summary>
        /// Creates a request with empty reason
        /// </summary>
        public static object CreateRequestWithEmptyReason(
            Shopilent.Domain.Sales.Enums.OrderStatus status) => new
        {
            Status = status,
            Reason = ""
        };

        /// <summary>
        /// Creates a request with whitespace reason
        /// </summary>
        public static object CreateRequestWithWhitespaceReason(
            Shopilent.Domain.Sales.Enums.OrderStatus status) => new
        {
            Status = status,
            Reason = "   "
        };

        /// <summary>
        /// Creates a request with maximum length reason (500 characters)
        /// </summary>
        public static object CreateRequestWithMaximumLengthReason(
            Shopilent.Domain.Sales.Enums.OrderStatus status) => new
        {
            Status = status,
            Reason = new string('A', 500)
        };

        /// <summary>
        /// Creates a request with reason exceeding maximum length (501 characters)
        /// </summary>
        public static object CreateRequestWithTooLongReason(
            Shopilent.Domain.Sales.Enums.OrderStatus status) => new
        {
            Status = status,
            Reason = new string('A', 501)
        };

        /// <summary>
        /// Creates a request with unicode characters in reason
        /// </summary>
        public static object CreateRequestWithUnicodeReason(
            Shopilent.Domain.Sales.Enums.OrderStatus status) => new
        {
            Status = status,
            Reason = "状態更新 - Status updated. Café Münchën™ 日本 ✓"
        };

        /// <summary>
        /// Creates a request with special characters in reason
        /// </summary>
        public static object CreateRequestWithSpecialCharactersReason(
            Shopilent.Domain.Sales.Enums.OrderStatus status) => new
        {
            Status = status,
            Reason = "Status updated: @#$%^&*() - Special test!"
        };

        /// <summary>
        /// Creates a request with multiline reason
        /// </summary>
        public static object CreateRequestWithMultilineReason(
            Shopilent.Domain.Sales.Enums.OrderStatus status) => new
        {
            Status = status,
            Reason = "Line 1: Status change initiated\nLine 2: Verification completed\nLine 3: Update confirmed"
        };

        /// <summary>
        /// Predefined status update reasons for common scenarios
        /// </summary>
        public static class CommonReasons
        {
            public const string ProcessingStarted = "Order processing has been initiated";
            public const string ShipmentPrepared = "Order prepared and ready for shipment";
            public const string QualityCheckPassed = "Quality check completed successfully";
            public const string ManualReview = "Order requires manual review";
            public const string AdminOverride = "Admin override for status update";
            public const string SystemUpdate = "Automated system status update";
            public const string CustomerRequested = "Status updated per customer request";
            public const string InventoryConfirmed = "Inventory availability confirmed";
        }
    }

    /// <summary>
    /// Order cancellation test scenarios
    /// </summary>
    public static class Cancellation
    {
        /// <summary>
        /// Creates a valid cancellation request with customizable parameters
        /// </summary>
        public static object CreateValidRequest(string? reason = null)
        {
            return new
            {
                Reason = reason
            };
        }

        /// <summary>
        /// Creates a cancellation request with a short reason
        /// </summary>
        public static object CreateRequestWithShortReason() => new
        {
            Reason = "Changed mind"
        };

        /// <summary>
        /// Creates a cancellation request with a detailed reason
        /// </summary>
        public static object CreateRequestWithDetailedReason() => new
        {
            Reason = "Customer changed their mind after reviewing the order details and decided to purchase alternative products instead."
        };

        /// <summary>
        /// Creates a cancellation request without reason (optional field)
        /// </summary>
        public static object CreateRequestWithoutReason() => new
        {
            Reason = (string?)null
        };

        /// <summary>
        /// Creates a cancellation request with empty reason
        /// </summary>
        public static object CreateRequestWithEmptyReason() => new
        {
            Reason = ""
        };

        /// <summary>
        /// Creates a cancellation request with whitespace reason
        /// </summary>
        public static object CreateRequestWithWhitespaceReason() => new
        {
            Reason = "   "
        };

        /// <summary>
        /// Creates a cancellation request with maximum length reason (500 characters)
        /// </summary>
        public static object CreateRequestWithMaximumLengthReason() => new
        {
            Reason = new string('A', 500)
        };

        /// <summary>
        /// Creates a cancellation request with reason exceeding maximum length (501 characters)
        /// </summary>
        public static object CreateRequestWithTooLongReason() => new
        {
            Reason = new string('A', 501)
        };

        /// <summary>
        /// Creates a cancellation request with unicode characters in reason
        /// </summary>
        public static object CreateRequestWithUnicodeReason() => new
        {
            Reason = "顧客がキャンセル - Customer cancelled. Café Münchën™ 日本 🛍️"
        };

        /// <summary>
        /// Creates a cancellation request with special characters in reason
        /// </summary>
        public static object CreateRequestWithSpecialCharactersReason() => new
        {
            Reason = "Order cancelled: @#$%^&*() - Special chars test!"
        };

        /// <summary>
        /// Creates a cancellation request with multiline reason
        /// </summary>
        public static object CreateRequestWithMultilineReason() => new
        {
            Reason = "Line 1: Customer dissatisfied\nLine 2: Product not as expected\nLine 3: Requesting full refund"
        };

        /// <summary>
        /// Predefined cancellation reasons for common scenarios
        /// </summary>
        public static class CommonReasons
        {
            public const string CustomerRequest = "Customer requested cancellation";
            public const string OutOfStock = "Product out of stock";
            public const string PaymentFailed = "Payment authorization failed";
            public const string DuplicateOrder = "Duplicate order detected";
            public const string FraudulentOrder = "Suspected fraudulent order";
            public const string CustomerNoResponse = "Customer not responding to order confirmation";
            public const string ShippingIssue = "Shipping address cannot be verified";
            public const string PriceError = "Pricing error in the order";
        }
    }

    /// <summary>
    /// Order partial refund test scenarios for ProcessOrderPartialRefund endpoint
    /// </summary>
    public static class PartialRefund
    {
        /// <summary>
        /// Creates a valid partial refund request with customizable parameters
        /// </summary>
        public static object CreateValidRequest(
            decimal? amount = null,
            string? currency = null,
            string? reason = null)
        {
            return new
            {
                Amount = amount ?? _faker.Random.Decimal(1, 100),
                Currency = currency ?? "USD",
                Reason = reason
            };
        }

        /// <summary>
        /// Creates a partial refund request with specific amount
        /// </summary>
        public static object CreateRequestWithAmount(decimal amount, string? reason = null) => new
        {
            Amount = amount,
            Currency = "USD",
            Reason = reason
        };

        /// <summary>
        /// Creates a partial refund request with specific currency
        /// </summary>
        public static object CreateRequestWithCurrency(string currency, decimal? amount = null) => new
        {
            Amount = amount ?? 50.00m,
            Currency = currency,
            Reason = (string?)null
        };

        /// <summary>
        /// Creates a partial refund request without reason (optional field)
        /// </summary>
        public static object CreateRequestWithoutReason(decimal? amount = null) => new
        {
            Amount = amount ?? 25.00m,
            Currency = "USD",
            Reason = (string?)null
        };

        /// <summary>
        /// Creates a partial refund request with empty reason
        /// </summary>
        public static object CreateRequestWithEmptyReason(decimal? amount = null) => new
        {
            Amount = amount ?? 25.00m,
            Currency = "USD",
            Reason = ""
        };

        /// <summary>
        /// Creates a partial refund request with whitespace reason
        /// </summary>
        public static object CreateRequestWithWhitespaceReason(decimal? amount = null) => new
        {
            Amount = amount ?? 25.00m,
            Currency = "USD",
            Reason = "   "
        };

        /// <summary>
        /// Creates a partial refund request with detailed reason
        /// </summary>
        public static object CreateRequestWithDetailedReason(decimal? amount = null) => new
        {
            Amount = amount ?? 30.00m,
            Currency = "USD",
            Reason = "Partial refund issued due to minor product defect. Customer agreed to keep item with partial compensation."
        };

        /// <summary>
        /// Creates a partial refund request with maximum length reason (500 characters)
        /// </summary>
        public static object CreateRequestWithMaximumLengthReason(decimal? amount = null) => new
        {
            Amount = amount ?? 25.00m,
            Currency = "USD",
            Reason = new string('A', 500)
        };

        /// <summary>
        /// Creates a partial refund request with reason exceeding maximum length (501 characters)
        /// </summary>
        public static object CreateRequestWithTooLongReason(decimal? amount = null) => new
        {
            Amount = amount ?? 25.00m,
            Currency = "USD",
            Reason = new string('A', 501)
        };

        /// <summary>
        /// Creates a partial refund request with unicode characters in reason
        /// </summary>
        public static object CreateRequestWithUnicodeReason(decimal? amount = null) => new
        {
            Amount = amount ?? 25.00m,
            Currency = "USD",
            Reason = "部分返金処理 - Partial refund processed. Café Münchën™ 日本 💰"
        };

        /// <summary>
        /// Creates a partial refund request with special characters in reason
        /// </summary>
        public static object CreateRequestWithSpecialCharactersReason(decimal? amount = null) => new
        {
            Amount = amount ?? 25.00m,
            Currency = "USD",
            Reason = "Partial refund: @#$%^&*() - Special chars test!"
        };

        /// <summary>
        /// Creates a partial refund request with multiline reason
        /// </summary>
        public static object CreateRequestWithMultilineReason(decimal? amount = null) => new
        {
            Amount = amount ?? 25.00m,
            Currency = "USD",
            Reason = "Line 1: Partial product issue\nLine 2: Customer accepted partial refund\nLine 3: Partial refund approved"
        };

        // Validation test cases for Amount
        public static object CreateRequestWithZeroAmount() => new
        {
            Amount = 0m,
            Currency = "USD",
            Reason = (string?)null
        };

        public static object CreateRequestWithNegativeAmount() => new
        {
            Amount = -50.00m,
            Currency = "USD",
            Reason = (string?)null
        };

        public static object CreateRequestWithExcessiveDecimalPlaces() => new
        {
            Amount = 25.999m, // 3 decimal places (only 2 allowed)
            Currency = "USD",
            Reason = (string?)null
        };

        public static object CreateRequestWithTwoDecimalPlaces() => new
        {
            Amount = 25.99m,
            Currency = "USD",
            Reason = (string?)null
        };

        public static object CreateRequestWithOneDecimalPlace() => new
        {
            Amount = 25.5m,
            Currency = "USD",
            Reason = (string?)null
        };

        public static object CreateRequestWithNoDecimalPlaces() => new
        {
            Amount = 25m,
            Currency = "USD",
            Reason = (string?)null
        };

        public static object CreateRequestWithVerySmallAmount() => new
        {
            Amount = 0.01m,
            Currency = "USD",
            Reason = (string?)null
        };

        public static object CreateRequestWithLargeAmount() => new
        {
            Amount = 99999.99m,
            Currency = "USD",
            Reason = (string?)null
        };

        // Validation test cases for Currency
        public static object CreateRequestWithEmptyCurrency(decimal? amount = null) => new
        {
            Amount = amount ?? 25.00m,
            Currency = "",
            Reason = (string?)null
        };

        public static object CreateRequestWithInvalidCurrencyLength(decimal? amount = null) => new
        {
            Amount = amount ?? 25.00m,
            Currency = "US", // Only 2 characters (should be 3)
            Reason = (string?)null
        };

        public static object CreateRequestWithLowercaseCurrency(decimal? amount = null) => new
        {
            Amount = amount ?? 25.00m,
            Currency = "usd", // Lowercase (should be uppercase)
            Reason = (string?)null
        };

        public static object CreateRequestWithInvalidCurrencyCharacters(decimal? amount = null) => new
        {
            Amount = amount ?? 25.00m,
            Currency = "U$D", // Contains special character
            Reason = (string?)null
        };

        public static object CreateRequestWithNumericCurrency(decimal? amount = null) => new
        {
            Amount = amount ?? 25.00m,
            Currency = "123",
            Reason = (string?)null
        };

        // Valid currency codes
        public static object CreateRequestWithEurCurrency(decimal? amount = null) => new
        {
            Amount = amount ?? 25.00m,
            Currency = "EUR",
            Reason = (string?)null
        };

        public static object CreateRequestWithGbpCurrency(decimal? amount = null) => new
        {
            Amount = amount ?? 25.00m,
            Currency = "GBP",
            Reason = (string?)null
        };

        public static object CreateRequestWithJpyCurrency(decimal? amount = null) => new
        {
            Amount = amount ?? 25.00m,
            Currency = "JPY",
            Reason = (string?)null
        };

        /// <summary>
        /// Predefined partial refund reasons for common scenarios
        /// </summary>
        public static class CommonReasons
        {
            public const string MinorDefect = "Minor product defect - partial compensation";
            public const string ShippingDelay = "Shipping delay - partial refund as apology";
            public const string MissingAccessory = "Missing accessory - partial refund issued";
            public const string PriceAdjustment = "Price adjustment - partial refund for price match";
            public const string PartialDamage = "Partial damage to product - customer keeping item";
            public const string ServiceIssue = "Service issue - partial compensation approved";
            public const string GoodwillGesture = "Goodwill gesture - partial refund for inconvenience";
            public const string IncompleteOrder = "Incomplete order - partial refund for missing items";
        }
    }

    /// <summary>
    /// Order refund test scenarios for ProcessOrderRefund endpoint
    /// </summary>
    public static class Refund
    {
        /// <summary>
        /// Creates a valid refund request with customizable reason
        /// </summary>
        public static object CreateValidRequest(string? reason = null)
        {
            return new
            {
                Reason = reason
            };
        }

        /// <summary>
        /// Creates a refund request with a short reason
        /// </summary>
        public static object CreateRequestWithShortReason() => new
        {
            Reason = "Product defect"
        };

        /// <summary>
        /// Creates a refund request with a detailed reason
        /// </summary>
        public static object CreateRequestWithDetailedReason() => new
        {
            Reason = "Product did not meet quality expectations and customer requested a full refund. Item will be returned to warehouse for inspection."
        };

        /// <summary>
        /// Creates a refund request without reason (optional field)
        /// </summary>
        public static object CreateRequestWithoutReason() => new
        {
            Reason = (string?)null
        };

        /// <summary>
        /// Creates a refund request with empty reason
        /// </summary>
        public static object CreateRequestWithEmptyReason() => new
        {
            Reason = ""
        };

        /// <summary>
        /// Creates a refund request with whitespace reason
        /// </summary>
        public static object CreateRequestWithWhitespaceReason() => new
        {
            Reason = "   "
        };

        /// <summary>
        /// Creates a refund request with maximum length reason (500 characters)
        /// </summary>
        public static object CreateRequestWithMaximumLengthReason() => new
        {
            Reason = new string('A', 500)
        };

        /// <summary>
        /// Creates a refund request with reason exceeding maximum length (501 characters)
        /// </summary>
        public static object CreateRequestWithTooLongReason() => new
        {
            Reason = new string('A', 501)
        };

        /// <summary>
        /// Creates a refund request with unicode characters in reason
        /// </summary>
        public static object CreateRequestWithUnicodeReason() => new
        {
            Reason = "返金処理 - Refund processed. Café Münchën™ 日本 💰"
        };

        /// <summary>
        /// Creates a refund request with special characters in reason
        /// </summary>
        public static object CreateRequestWithSpecialCharactersReason() => new
        {
            Reason = "Refund issued: @#$%^&*() - Special chars test!"
        };

        /// <summary>
        /// Creates a refund request with multiline reason
        /// </summary>
        public static object CreateRequestWithMultilineReason() => new
        {
            Reason = "Line 1: Product quality issue\nLine 2: Customer dissatisfied\nLine 3: Full refund approved"
        };

        /// <summary>
        /// Predefined refund reasons for common scenarios
        /// </summary>
        public static class CommonReasons
        {
            public const string CustomerRequest = "Customer requested refund";
            public const string DefectiveProduct = "Defective product";
            public const string WrongItemShipped = "Wrong item was shipped";
            public const string ProductNotAsDescribed = "Product not as described";
            public const string DamagedInShipping = "Product damaged during shipping";
            public const string QualityNotAcceptable = "Product quality not acceptable";
            public const string LateDelivery = "Late delivery - customer refund approved";
            public const string CustomerChangedMind = "Customer changed mind - refund approved";
            public const string DuplicatePayment = "Duplicate payment detected - refund issued";
            public const string PriceAdjustment = "Price adjustment refund";
        }
    }
}
