using Bogus;

namespace Shopilent.API.IntegrationTests.Endpoints.Identity.RefreshToken.V1;

public static class RefreshTokenTestDataV1
{
    private static readonly Faker _faker = new();

    // Core valid refresh token request generator
    public static object CreateValidRequest(string? refreshToken = null)
    {
        return new
        {
            RefreshToken = refreshToken ?? _faker.Random.AlphaNumeric(64)
        };
    }

    // Validation test cases
    public static object CreateRequestWithEmptyRefreshToken() => new
    {
        RefreshToken = ""
    };

    public static object CreateRequestWithNullRefreshToken() => new
    {
        RefreshToken = (string?)null
    };

    public static object CreateRequestWithWhitespaceRefreshToken() => new
    {
        RefreshToken = "   "
    };

    public static object CreateRequestWithInvalidRefreshToken() => new
    {
        RefreshToken = "invalid-refresh-token-format"
    };

    public static object CreateRequestWithExpiredRefreshToken() => new
    {
        RefreshToken = "expired.refresh.token.that.is.no.longer.valid.in.the.system"
    };

    public static object CreateRequestWithRevokedRefreshToken() => new
    {
        RefreshToken = "revoked.refresh.token.that.has.been.invalidated.by.logout"
    };

    // Security test cases
    public static class SecurityTests
    {
        public static object CreateSqlInjectionAttempt() => new
        {
            RefreshToken = "'; DROP TABLE RefreshTokens; --"
        };

        public static object CreateXssAttempt() => new
        {
            RefreshToken = "<script>alert('xss')</script>"
        };

        public static object CreateLongTokenAttack() => new
        {
            RefreshToken = new string('A', 10000) // Extremely long token
        };

        public static object CreateMalformedTokenAttempt() => new
        {
            RefreshToken = "malformed.token.with.special.chars!@#$%^&*()_+-=[]{}|;':\",./<>?"
        };
    }

    // Edge cases
    public static class EdgeCases
    {
        public static object CreateRequestWithUnicodeCharacters() => new
        {
            RefreshToken = "tökën.wîth.ünícödé.chärāctërs"
        };

        public static object CreateRequestWithNumericToken() => new
        {
            RefreshToken = "1234567890"
        };

        public static object CreateRequestWithBase64Token() => new
        {
            RefreshToken = Convert.ToBase64String(_faker.Random.Bytes(48))
        };

        public static object CreateRequestWithGuidToken() => new
        {
            RefreshToken = Guid.NewGuid().ToString()
        };

        public static object CreateRequestWithJwtLikeToken() => new
        {
            RefreshToken = $"{_faker.Random.AlphaNumeric(20)}.{_faker.Random.AlphaNumeric(30)}.{_faker.Random.AlphaNumeric(25)}"
        };
    }

    // Boundary tests
    public static class BoundaryTests
    {
        public static object CreateRequestWithMinimumLengthToken() => new
        {
            RefreshToken = _faker.Random.AlphaNumeric(1) // Minimum length
        };

        public static object CreateRequestWithTypicalLengthToken() => new
        {
            RefreshToken = _faker.Random.AlphaNumeric(64) // Typical refresh token length
        };

        public static object CreateRequestWithMaximumLengthToken() => new
        {
            RefreshToken = _faker.Random.AlphaNumeric(255) // Maximum allowed length per validator
        };

        public static object CreateRequestWithTooLongToken() => new
        {
            RefreshToken = _faker.Random.AlphaNumeric(512) // Exceeds maximum length - should return validation error
        };
    }

    // Test scenarios for different token states
    public static class TokenStates
    {
        public static object CreateRequestWithRecentlyIssuedToken() => new
        {
            RefreshToken = _faker.Random.AlphaNumeric(64)
        };

        public static object CreateRequestWithNearExpiryToken() => new
        {
            RefreshToken = _faker.Random.AlphaNumeric(64)
        };

        public static object CreateRequestWithAlreadyUsedToken() => new
        {
            RefreshToken = "already.used.refresh.token.that.should.be.invalid"
        };
    }
}