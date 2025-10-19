using Bogus;

namespace Shopilent.API.IntegrationTests.Endpoints.Identity.ResetPassword.V1;

public static class ResetPasswordTestDataV1
{
    private static readonly Faker _faker = new();

    // Core valid request generator
    public static object CreateValidRequest(
        string? token = null,
        string? newPassword = null,
        string? confirmPassword = null)
    {
        var password = newPassword ?? "ValidPassword123!";
        return new
        {
            Token = token ?? GenerateValidToken(),
            NewPassword = password,
            ConfirmPassword = confirmPassword ?? password
        };
    }

    // Special method for theory test that preserves null values
    public static object CreateRequestWithSpecificToken(string? token)
    {
        return new
        {
            Token = token,
            NewPassword = "ValidPassword123!",
            ConfirmPassword = "ValidPassword123!"
        };
    }

    // Validation test cases
    public static object CreateRequestWithEmptyToken() => new
    {
        Token = "",
        NewPassword = "ValidPassword123!",
        ConfirmPassword = "ValidPassword123!"
    };

    public static object CreateRequestWithNullToken() => new
    {
        Token = (string?)null,
        NewPassword = "ValidPassword123!",
        ConfirmPassword = "ValidPassword123!"
    };

    public static object CreateRequestWithEmptyPassword() => new
    {
        Token = GenerateValidToken(),
        NewPassword = "",
        ConfirmPassword = ""
    };

    public static object CreateRequestWithNullPassword() => new
    {
        Token = GenerateValidToken(),
        NewPassword = (string?)null,
        ConfirmPassword = (string?)null
    };

    public static object CreateRequestWithWeakPassword() => new
    {
        Token = GenerateValidToken(),
        NewPassword = "weak",
        ConfirmPassword = "weak"
    };

    public static object CreateRequestWithPasswordMissingUppercase() => new
    {
        Token = GenerateValidToken(),
        NewPassword = "password123!",
        ConfirmPassword = "password123!"
    };

    public static object CreateRequestWithPasswordMissingLowercase() => new
    {
        Token = GenerateValidToken(),
        NewPassword = "PASSWORD123!",
        ConfirmPassword = "PASSWORD123!"
    };

    public static object CreateRequestWithPasswordMissingNumber() => new
    {
        Token = GenerateValidToken(),
        NewPassword = "Password!",
        ConfirmPassword = "Password!"
    };

    public static object CreateRequestWithPasswordMissingSpecialChar() => new
    {
        Token = GenerateValidToken(),
        NewPassword = "Password123",
        ConfirmPassword = "Password123"
    };

    public static object CreateRequestWithMismatchedPasswords() => new
    {
        Token = GenerateValidToken(),
        NewPassword = "ValidPassword123!",
        ConfirmPassword = "DifferentPassword456@"
    };

    public static object CreateRequestWithEmptyConfirmPassword() => new
    {
        Token = GenerateValidToken(),
        NewPassword = "ValidPassword123!",
        ConfirmPassword = ""
    };

    // Boundary value testing
    public static class BoundaryTests
    {
        public static object CreateRequestWithMinimumPasswordLength() => new
        {
            Token = GenerateValidToken(),
            NewPassword = "Pass123!",
            ConfirmPassword = "Pass123!"
        };

        public static object CreateRequestWithMaximumPasswordLength() => new
        {
            Token = GenerateValidToken(),
            NewPassword = new string('A', 50) + "123!",
            ConfirmPassword = new string('A', 50) + "123!"
        };

        public static object CreateRequestWithSevenCharacterPassword() => new
        {
            Token = GenerateValidToken(),
            NewPassword = "Pass12!",
            ConfirmPassword = "Pass12!"
        };
    }

    // Edge cases
    public static class EdgeCases
    {
        public static object CreateRequestWithUnicodePassword() => new
        {
            Token = GenerateValidToken(),
            NewPassword = "Пароль123!",
            ConfirmPassword = "Пароль123!"
        };

        public static object CreateRequestWithSpecialCharactersPassword() => new
        {
            Token = GenerateValidToken(),
            NewPassword = "P@$$w0rd!#$%",
            ConfirmPassword = "P@$$w0rd!#$%"
        };

        public static object CreateRequestWithWhitespaceInPassword() => new
        {
            Token = GenerateValidToken(),
            NewPassword = "Pass word123!",
            ConfirmPassword = "Pass word123!"
        };

        public static object CreateRequestWithVeryLongToken() => new
        {
            Token = new string('A', 1000),
            NewPassword = "ValidPassword123!",
            ConfirmPassword = "ValidPassword123!"
        };

        public static object CreateRequestWithSpecialCharactersInToken() => new
        {
            Token = "token-with-special-chars!@#$%^&*()",
            NewPassword = "ValidPassword123!",
            ConfirmPassword = "ValidPassword123!"
        };
    }

    // Authentication test cases
    public static class AuthenticationTests
    {
        public static object CreateRequestWithExpiredToken() => new
        {
            Token = "expired-token-12345",
            NewPassword = "ValidPassword123!",
            ConfirmPassword = "ValidPassword123!"
        };

        public static object CreateRequestWithInvalidToken() => new
        {
            Token = "invalid-token-format",
            NewPassword = "ValidPassword123!",
            ConfirmPassword = "ValidPassword123!"
        };

        public static object CreateRequestWithMalformedToken() => new
        {
            Token = "malformed.token.123",
            NewPassword = "ValidPassword123!",
            ConfirmPassword = "ValidPassword123!"
        };
    }

    // Security test cases
    public static class SecurityTests
    {
        public static object CreateRequestWithSqlInjectionInPassword() => new
        {
            Token = GenerateValidToken(),
            NewPassword = "Password'; DROP TABLE Users; --123!",
            ConfirmPassword = "Password'; DROP TABLE Users; --123!"
        };

        public static object CreateRequestWithXssInPassword() => new
        {
            Token = GenerateValidToken(),
            NewPassword = "Pass<script>alert('xss')</script>123!",
            ConfirmPassword = "Pass<script>alert('xss')</script>123!"
        };
    }

    // Helper method to generate a valid-looking token
    private static string GenerateValidToken()
    {
        return _faker.Random.AlphaNumeric(64);
    }

    // Generate a realistic JWT-like token for more advanced testing
    public static string GenerateJwtLikeToken()
    {
        var header = _faker.Random.AlphaNumeric(36);
        var payload = _faker.Random.AlphaNumeric(128);
        var signature = _faker.Random.AlphaNumeric(86);
        return $"{header}.{payload}.{signature}";
    }
}