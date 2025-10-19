using Bogus;

namespace Shopilent.API.IntegrationTests.Endpoints.Identity.Logout.V1;

public static class LogoutTestDataV1
{
    private static readonly Faker _faker = new();

    // Core valid logout request generator
    public static object CreateValidRequest(
        string? refreshToken = null,
        string? reason = null)
    {
        return new
        {
            RefreshToken = refreshToken ?? _faker.Random.String2(40),
            Reason = reason ?? "User logged out"
        };
    }

    // Validation test cases
    public static object CreateRequestWithEmptyRefreshToken() => new
    {
        RefreshToken = "",
        Reason = "User logged out"
    };

    public static object CreateRequestWithNullRefreshToken() => new
    {
        RefreshToken = (string?)null,
        Reason = "User logged out"
    };

    public static object CreateRequestWithEmptyReason() => new
    {
        RefreshToken = _faker.Random.String2(40),
        Reason = ""
    };

    public static object CreateRequestWithNullReason() => new
    {
        RefreshToken = _faker.Random.String2(40),
        Reason = (string?)null
    };

    public static object CreateRequestWithInvalidRefreshToken() => new
    {
        RefreshToken = "invalid-refresh-token",
        Reason = "User logged out"
    };

    public static object CreateRequestWithExpiredRefreshToken() => new
    {
        RefreshToken = _faker.Random.String2(40), // This would be an expired token
        Reason = "User logged out"
    };

    // Security test cases
    public static class SecurityTests
    {
        public static object CreateSqlInjectionAttempt() => new
        {
            RefreshToken = "'; DROP TABLE RefreshTokens; --",
            Reason = "User logged out"
        };

        public static object CreateXssAttempt() => new
        {
            RefreshToken = "<script>alert('xss')</script>",
            Reason = "User logged out"
        };

        public static object CreateLongRefreshTokenAttack() => new
        {
            RefreshToken = new string('a', 10000),
            Reason = "User logged out"
        };

        public static object CreateLongReasonAttack() => new
        {
            RefreshToken = _faker.Random.String2(40),
            Reason = new string('A', 10000)
        };
    }

    // Edge cases
    public static class EdgeCases
    {
        public static object CreateRequestWithUnicodeReason() => new
        {
            RefreshToken = _faker.Random.String2(40),
            Reason = "Üser lögöut with spëcial çharacters"
        };

        public static object CreateRequestWithWhitespaceRefreshToken() => new
        {
            RefreshToken = "  " + _faker.Random.String2(40) + "  ",
            Reason = "User logged out"
        };

        public static object CreateRequestWithWhitespaceReason() => new
        {
            RefreshToken = _faker.Random.String2(40),
            Reason = "  User logged out  "
        };

        public static object CreateRequestWithOnlyWhitespaceReason() => new
        {
            RefreshToken = _faker.Random.String2(40),
            Reason = "   "
        };

        public static object CreateRequestWithSpecialCharactersInToken() => new
        {
            RefreshToken = "abc!@#$%^&*()_+-=[]{}|;':\",./<>?",
            Reason = "User logged out"
        };
    }

    // Boundary tests
    public static class BoundaryTests
    {
        public static object CreateRequestWithMinimumValidRefreshToken() => new
        {
            RefreshToken = "a", // Minimum length token
            Reason = "User logged out"
        };

        public static object CreateRequestWithMaximumValidRefreshToken() => new
        {
            RefreshToken = new string('a', 255), // Assuming 255 is the maximum length
            Reason = "User logged out"
        };

        public static object CreateRequestWithMinimumValidReason() => new
        {
            RefreshToken = _faker.Random.String2(40),
            Reason = "a" // Minimum length reason
        };

        public static object CreateRequestWithMaximumValidReason() => new
        {
            RefreshToken = _faker.Random.String2(40),
            Reason = new string('A', 500) // Assuming 500 is the maximum length
        };
    }

    // Common logout reasons
    public static class LogoutReasons
    {
        public static object CreateRequestWithUserInitiatedLogout() => new
        {
            RefreshToken = _faker.Random.String2(40),
            Reason = "User initiated logout"
        };

        public static object CreateRequestWithSessionTimeout() => new
        {
            RefreshToken = _faker.Random.String2(40),
            Reason = "Session timeout"
        };

        public static object CreateRequestWithSecurityLogout() => new
        {
            RefreshToken = _faker.Random.String2(40),
            Reason = "Security logout - suspicious activity detected"
        };

        public static object CreateRequestWithAdminLogout() => new
        {
            RefreshToken = _faker.Random.String2(40),
            Reason = "Administrative logout"
        };

        public static object CreateRequestWithPasswordChange() => new
        {
            RefreshToken = _faker.Random.String2(40),
            Reason = "Password changed - logout required"
        };
    }
}