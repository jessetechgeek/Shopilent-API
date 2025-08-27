using Bogus;

namespace Shopilent.API.IntegrationTests.Endpoints.Identity.Login.V1;

public static class LoginTestDataV1
{
    private static readonly Faker _faker = new();

    // Core valid login request generator
    public static object CreateValidRequest(
        string? email = null,
        string? password = null)
    {
        return new
        {
            Email = email ?? "test@example.com",
            Password = password ?? "Password123!",
            RememberMe = false
        };
    }

    // Admin user credentials
    public static object CreateAdminLoginRequest() => new
    {
        Email = "admin@shopilent.com",
        Password = "AdminPassword123!",
        RememberMe = false
    };

    // Customer user credentials  
    public static object CreateCustomerLoginRequest() => new
    {
        Email = "customer@example.com",
        Password = "CustomerPassword123!",
        RememberMe = false
    };

    // Remember me enabled
    public static object CreateRememberMeRequest() => new
    {
        Email = "test@example.com",
        Password = "Password123!",
        RememberMe = true
    };

    // Validation test cases
    public static object CreateRequestWithEmptyEmail() => new
    {
        Email = "",
        Password = "Password123!",
        RememberMe = false
    };

    public static object CreateRequestWithNullEmail() => new
    {
        Email = (string?)null,
        Password = "Password123!",
        RememberMe = false
    };

    public static object CreateRequestWithEmptyPassword() => new
    {
        Email = "test@example.com",
        Password = "",
        RememberMe = false
    };

    public static object CreateRequestWithNullPassword() => new
    {
        Email = "test@example.com",
        Password = (string?)null,
        RememberMe = false
    };

    public static object CreateRequestWithInvalidEmailFormat() => new
    {
        Email = "invalid-email-format",
        Password = "Password123!",
        RememberMe = false
    };

    public static object CreateRequestWithNonExistentUser() => new
    {
        Email = "nonexistent@example.com",
        Password = "Password123!",
        RememberMe = false
    };

    public static object CreateRequestWithWrongPassword() => new
    {
        Email = "test@example.com",
        Password = "WrongPassword123!",
        RememberMe = false
    };

    // Security test cases
    public static class SecurityTests
    {
        public static object CreateSqlInjectionAttempt() => new
        {
            Email = "admin@example.com'; DROP TABLE Users; --",
            Password = "Password123!",
            RememberMe = false
        };

        public static object CreateXssAttempt() => new
        {
            Email = "<script>alert('xss')</script>@example.com",
            Password = "Password123!",
            RememberMe = false
        };

        public static object CreateLongEmailAttack() => new
        {
            Email = new string('a', 1000) + "@example.com",
            Password = "Password123!",
            RememberMe = false
        };

        public static object CreateLongPasswordAttack() => new
        {
            Email = "test@example.com",
            Password = new string('A', 1000),
            RememberMe = false
        };
    }

    // Edge cases
    public static class EdgeCases
    {
        public static object CreateRequestWithUnicodeEmail() => new
        {
            Email = "tëst@éxämplé.com",
            Password = "Password123!",
            RememberMe = false
        };

        public static object CreateRequestWithPlusAddressing() => new
        {
            Email = "test+tag@example.com",
            Password = "Password123!",
            RememberMe = false
        };

        public static object CreateRequestWithUppercaseEmail() => new
        {
            Email = "TEST@EXAMPLE.COM",
            Password = "Password123!",
            RememberMe = false
        };

        public static object CreateRequestWithMixedCaseEmail() => new
        {
            Email = "TeSt@ExAmPlE.CoM",
            Password = "Password123!",
            RememberMe = false
        };

        public static object CreateRequestWithWhitespaceEmail() => new
        {
            Email = "  test@example.com  ",
            Password = "Password123!",
            RememberMe = false
        };

        public static object CreateRequestWithWhitespacePassword() => new
        {
            Email = "test@example.com",
            Password = "  Password123!  ",
            RememberMe = false
        };
    }

    // Boundary tests
    public static class BoundaryTests
    {
        public static object CreateRequestWithMinimumValidEmail() => new
        {
            Email = "a@b.co", // Minimum valid email format
            Password = "Password123!",
            RememberMe = false
        };

        public static object CreateRequestWithMaximumValidEmail() => new
        {
            Email = new string('a', 64) + "@" + new string('b', 63) + ".com", // Near maximum email length
            Password = "Password123!",
            RememberMe = false
        };

        public static object CreateRequestWithMinimumPassword() => new
        {
            Email = "test@example.com",
            Password = "Pass1!", // Assuming minimum password length is 6
            RememberMe = false
        };
    }
}