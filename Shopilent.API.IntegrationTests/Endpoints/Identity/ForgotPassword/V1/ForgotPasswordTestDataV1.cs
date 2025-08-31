using Bogus;

namespace Shopilent.API.IntegrationTests.Endpoints.Identity.ForgotPassword.V1;

public static class ForgotPasswordTestDataV1
{
    private static readonly Faker _faker = new();

    // Core valid request generator
    public static object CreateValidRequest(string? email = null)
    {
        return new
        {
            Email = email ?? _faker.Internet.Email()
        };
    }

    // Special method for theory test that preserves null values
    public static object CreateRequestWithSpecificEmail(string? email)
    {
        return new
        {
            Email = email
        };
    }

    // Validation test cases
    public static object CreateRequestWithEmptyEmail() => new
    {
        Email = ""
    };

    public static object CreateRequestWithNullEmail() => new
    {
        Email = (string?)null
    };

    public static object CreateRequestWithWhitespaceEmail() => new
    {
        Email = "   "
    };

    public static object CreateRequestWithInvalidEmailFormat() => new
    {
        Email = "not-an-email"
    };

    public static object CreateRequestWithInvalidEmailMissingDomain() => new
    {
        Email = "user@"
    };

    public static object CreateRequestWithInvalidEmailMissingAt() => new
    {
        Email = "userexample.com"
    };

    // Boundary value testing
    public static class BoundaryTests
    {
        public static object CreateRequestWithMinimumValidEmail() => new
        {
            Email = "a@b.co" // Shortest valid email format
        };

        public static object CreateRequestWithLongValidEmail() => new
        {
            Email = $"{new string('a', 50)}@{new string('b', 50)}.{new string('c', 10)}" // Very long but valid email
        };

        public static object CreateRequestWithMaximumLocalPart() => new
        {
            Email = $"{new string('a', 64)}@example.com" // Max local part length (64 chars)
        };

        public static object CreateRequestWithTooLongLocalPart() => new
        {
            Email = $"{new string('a', 65)}@example.com" // Over max local part length
        };
    }

    // Edge cases
    public static class EdgeCases
    {
        public static object CreateRequestWithUnicodeEmail() => new
        {
            Email = "tëst@üñíçödé.com" // Unicode characters
        };

        public static object CreateRequestWithSpecialCharacters() => new
        {
            Email = "test+tag@example-domain.co.uk" // Valid special characters
        };

        public static object CreateRequestWithUppercaseEmail() => new
        {
            Email = "TEST.USER@EXAMPLE.COM" // Uppercase email
        };

        public static object CreateRequestWithMixedCaseEmail() => new
        {
            Email = "TeSt.UsEr@ExAmPlE.cOm" // Mixed case email
        };

        public static object CreateRequestWithSubdomainEmail() => new
        {
            Email = "user@subdomain.example.com" // Subdomain
        };

        public static object CreateRequestWithNumericDomain() => new
        {
            Email = "user@123.456.789.012" // Numeric domain (IP-like but invalid)
        };

        public static object CreateRequestWithLeadingTrailingSpaces() => new
        {
            Email = "  user@example.com  " // Leading/trailing spaces
        };
    }

    // Common test email addresses
    public static class TestEmails
    {
        public const string ExistingUserEmail = "admin@shopilent.com";
        public const string AnotherExistingUserEmail = "customer@shopilent.com";
        public const string NonExistentEmail = "nonexistent@example.com";
        public const string TestEmail = "test@example.com";
        
        public static object CreateRequestWithExistingEmail() => new
        {
            Email = ExistingUserEmail
        };

        public static object CreateRequestWithNonExistentEmail() => new
        {
            Email = NonExistentEmail
        };

        public static object CreateRequestWithTestEmail() => new
        {
            Email = TestEmail
        };
    }

    // Security test cases
    public static class SecurityTests
    {
        public static object CreateRequestWithSqlInjectionAttempt() => new
        {
            Email = "'; DROP TABLE Users; --@example.com"
        };

        public static object CreateRequestWithXssAttempt() => new
        {
            Email = "<script>alert('xss')</script>@example.com"
        };

        public static object CreateRequestWithLdapInjectionAttempt() => new
        {
            Email = "user@example.com)(|(password=*))"
        };

        public static object CreateRequestWithCommandInjectionAttempt() => new
        {
            Email = "user@example.com; rm -rf /"
        };
    }

    // Performance testing
    public static class PerformanceTests
    {
        public static IEnumerable<object> GenerateMultipleValidRequests(int count = 10)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new
                {
                    Email = _faker.Internet.Email()
                };
            }
        }

        public static object CreateRequestWithVeryLongEmail() => new
        {
            Email = $"{new string('a', 200)}@{new string('b', 200)}.com" // Very long email
        };
    }
}