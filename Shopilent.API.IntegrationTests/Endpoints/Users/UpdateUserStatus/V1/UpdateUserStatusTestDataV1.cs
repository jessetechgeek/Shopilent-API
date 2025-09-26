using Bogus;

namespace Shopilent.API.IntegrationTests.Endpoints.Users.UpdateUserStatus.V1;

public static class UpdateUserStatusTestDataV1
{
    private static readonly Faker _faker = new();

    // Core valid request generators
    public static object CreateValidRequest(bool? isActive = null)
    {
        return new
        {
            IsActive = isActive ?? _faker.Random.Bool()
        };
    }

    // Specific status requests
    public static object CreateActivateRequest() => new
    {
        IsActive = true
    };

    public static object CreateDeactivateRequest() => new
    {
        IsActive = false
    };

    // Valid boolean variations
    public static object CreateTrueRequest() => new
    {
        IsActive = true
    };

    public static object CreateFalseRequest() => new
    {
        IsActive = false
    };

    // Edge cases for boolean binding
    public static class EdgeCases
    {
        public static object CreateRequestWithStringTrue() => new
        {
            IsActive = "true" // Should be converted to boolean true
        };

        public static object CreateRequestWithStringFalse() => new
        {
            IsActive = "false" // Should be converted to boolean false
        };

        public static object CreateRequestWithNumericTrue() => new
        {
            IsActive = 1 // Should be converted to boolean true
        };

        public static object CreateRequestWithNumericFalse() => new
        {
            IsActive = 0 // Should be converted to boolean false
        };

        public static object CreateRequestWithInvalidString() => new
        {
            IsActive = "invalid" // Should fail boolean binding
        };

        public static object CreateRequestWithNullValue() => new
        {
            IsActive = (bool?)null // Should fail validation
        };

        // Malformed JSON request bodies (syntax errors)
        public static string CreateEmptyJsonRequest() => "{}"; // Valid JSON - defaults IsActive to false
        public static string CreateInvalidJsonRequest() => "{ \"IsActive\": }"; // Missing value
        public static string CreateMalformedJsonRequest() => "{ IsActive: true"; // Missing quotes and closing brace
        public static string CreateIncompleteJsonRequest() => "{ \"IsActive\":"; // Incomplete JSON
        public static string CreateInvalidBracesRequest() => "{ \"IsActive\": true ]"; // Mismatched brackets
    }

    // Security tests
    public static class SecurityTests
    {
        public static object CreateSqlInjectionAttempt() => new
        {
            IsActive = "'; DROP TABLE Users; --"
        };

        public static object CreateXssAttempt() => new
        {
            IsActive = "<script>alert('xss')</script>"
        };

        public static object CreateCommandInjectionAttempt() => new
        {
            IsActive = "$(rm -rf /)"
        };

        public static object CreateLdapInjectionAttempt() => new
        {
            IsActive = "admin)(|(password=*))"
        };

        public static object CreateNoSqlInjectionAttempt() => new
        {
            IsActive = "'; return db.users.drop(); //"
        };

        // Path traversal attempts
        public static object CreatePathTraversalAttempt() => new
        {
            IsActive = "../../etc/passwd"
        };

        // Large payload test
        public static object CreateLargePayloadAttempt() => new
        {
            IsActive = new string('A', 10000) // Very large string
        };

        // Unicode and special characters
        public static object CreateUnicodeAttempt() => new
        {
            IsActive = "𝕿𝖊𝖘𝖙 🔥 𝔘𝔫𝔦𝔠𝔬𝔡𝔢"
        };
    }

    // Concurrency test helpers
    public static class ConcurrencyTests
    {
        public static List<object> CreateMultipleActivateRequests(int count = 5)
        {
            return Enumerable.Range(0, count)
                .Select(_ => CreateActivateRequest())
                .ToList();
        }

        public static List<object> CreateMultipleDeactivateRequests(int count = 5)
        {
            return Enumerable.Range(0, count)
                .Select(_ => CreateDeactivateRequest())
                .ToList();
        }

        public static List<object> CreateAlternatingStatusRequests(int count = 10)
        {
            return Enumerable.Range(0, count)
                .Select(i => CreateValidRequest(i % 2 == 0))
                .ToList();
        }

        // Optimized for concurrency testing - smaller load to avoid timeouts
        public static List<object> CreateConcurrencyTestRequests()
        {
            return new List<object>
            {
                CreateActivateRequest(),   // First: Activate
                CreateDeactivateRequest(), // Second: Deactivate
                CreateActivateRequest()    // Third: Activate again
            };
        }
    }

    // Test helper methods
    public static bool GetRandomBool()
    {
        return _faker.Random.Bool();
    }

    public static bool GetOppositeBool(bool currentStatus)
    {
        return !currentStatus;
    }

    public static string GetStatusMessage(bool isActive)
    {
        return isActive ? "active" : "inactive";
    }

    public static string GetExpectedSuccessMessage(bool isActive)
    {
        return $"User status updated to {GetStatusMessage(isActive)}";
    }

    // Invalid GUID test data
    public static class InvalidGuids
    {
        public static string EmptyGuid => Guid.Empty.ToString();
        public static string InvalidGuidFormat => "not-a-guid";
        public static string PartialGuid => "123-456";
        public static string TooLongGuid => "12345678-1234-1234-1234-123456789012-extra";
        public static string SpecialCharGuid => "special-chars-!@#$-guid";
    }

    // Test data for boundary testing
    public static class BoundaryTests
    {
        // Test with native JSON boolean values that should work
        public static readonly object[] ValidBooleanValues =
        {
            new { IsActive = true },
            new { IsActive = false }
        };

        // Test with values that should fail boolean binding (string/numeric representations are invalid)
        public static readonly object[] InvalidBooleanValues =
        {
            new { IsActive = "true" },      // String representation should fail
            new { IsActive = "false" },     // String representation should fail
            new { IsActive = "True" },      // String representation should fail
            new { IsActive = "False" },     // String representation should fail
            new { IsActive = "TRUE" },      // String representation should fail
            new { IsActive = "FALSE" },     // String representation should fail
            new { IsActive = 1 },           // Numeric representation should fail
            new { IsActive = 0 },           // Numeric representation should fail
            new { IsActive = "maybe" },     // Invalid string
            new { IsActive = "yes" },       // Invalid string
            new { IsActive = "no" },        // Invalid string
            new { IsActive = 2 },           // Invalid number
            new { IsActive = -1 },          // Invalid number
            new { IsActive = "null" },      // Invalid string
            new { IsActive = "" }           // Empty string
        };
    }
}