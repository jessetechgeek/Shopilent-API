using Bogus;

namespace Shopilent.API.IntegrationTests.Endpoints.Users.ChangePassword.V1;

public static class ChangePasswordTestDataV1
{
    private static readonly Faker _faker = new();

    // Core valid request generator
    public static object CreateValidRequest(
        string? currentPassword = null,
        string? newPassword = null,
        string? confirmPassword = null)
    {
        var validNewPassword = newPassword ?? GenerateValidPassword();
        return new
        {
            CurrentPassword = currentPassword ?? "Customer123!",
            NewPassword = validNewPassword,
            ConfirmPassword = confirmPassword ?? validNewPassword
        };
    }

    // Validation test cases
    public static object CreateRequestWithEmptyCurrentPassword() => new
    {
        CurrentPassword = "",
        NewPassword = GenerateValidPassword(),
        ConfirmPassword = GenerateValidPassword()
    };

    public static object CreateRequestWithNullCurrentPassword() => new
    {
        CurrentPassword = (string?)null,
        NewPassword = GenerateValidPassword(),
        ConfirmPassword = GenerateValidPassword()
    };

    public static object CreateRequestWithEmptyNewPassword() => new
    {
        CurrentPassword = "Customer123!",
        NewPassword = "",
        ConfirmPassword = ""
    };

    public static object CreateRequestWithNullNewPassword() => new
    {
        CurrentPassword = "Customer123!",
        NewPassword = (string?)null,
        ConfirmPassword = (string?)null
    };

    public static object CreateRequestWithEmptyConfirmPassword() => new
    {
        CurrentPassword = "Customer123!",
        NewPassword = GenerateValidPassword(),
        ConfirmPassword = ""
    };

    public static object CreateRequestWithNullConfirmPassword() => new
    {
        CurrentPassword = "Customer123!",
        NewPassword = GenerateValidPassword(),
        ConfirmPassword = (string?)null
    };

    public static object CreateRequestWithMismatchedPasswords() => new
    {
        CurrentPassword = "Customer123!",
        NewPassword = "NewPassword123!",
        ConfirmPassword = "DifferentPassword123!"
    };

    public static object CreateRequestWithSameCurrentAndNewPassword() => new
    {
        CurrentPassword = "SamePassword123!",
        NewPassword = "SamePassword123!",
        ConfirmPassword = "SamePassword123!"
    };

    public static object CreateRequestWithShortNewPassword() => new
    {
        CurrentPassword = "Customer123!",
        NewPassword = "Short1!",
        ConfirmPassword = "Short1!"
    };

    public static object CreateRequestWithWeakNewPassword() => new
    {
        CurrentPassword = "Customer123!",
        NewPassword = "weakpassword",
        ConfirmPassword = "weakpassword"
    };

    // Password strength validation test cases
    public static class PasswordStrengthTests
    {
        public static object CreateRequestWithNoUppercaseNewPassword() => new
        {
            CurrentPassword = "Customer123!",
            NewPassword = "newpassword123!",
            ConfirmPassword = "newpassword123!"
        };

        public static object CreateRequestWithNoLowercaseNewPassword() => new
        {
            CurrentPassword = "Customer123!",
            NewPassword = "NEWPASSWORD123!",
            ConfirmPassword = "NEWPASSWORD123!"
        };

        public static object CreateRequestWithNoNumberNewPassword() => new
        {
            CurrentPassword = "Customer123!",
            NewPassword = "NewPassword!",
            ConfirmPassword = "NewPassword!"
        };

        public static object CreateRequestWithNoSpecialCharNewPassword() => new
        {
            CurrentPassword = "Customer123!",
            NewPassword = "NewPassword123",
            ConfirmPassword = "NewPassword123"
        };
    }

    // Boundary value testing
    public static class BoundaryTests
    {
        public static object CreateRequestWithMinimumValidPassword() => new
        {
            CurrentPassword = "Customer123!",
            NewPassword = "MinVal1!",  // 8 characters (minimum)
            ConfirmPassword = "MinVal1!"
        };

        public static object CreateRequestWithSevenCharacterPassword() => new
        {
            CurrentPassword = "Customer123!",
            NewPassword = "Short1!",  // 7 characters (below minimum)
            ConfirmPassword = "Short1!"
        };

        public static object CreateRequestWithVeryLongPassword() => new
        {
            CurrentPassword = "Customer123!",
            NewPassword = GeneratePasswordWithLength(128),
            ConfirmPassword = GeneratePasswordWithLength(128)
        };

        public static object CreateRequestWithExtremelyLongPassword() => new
        {
            CurrentPassword = "Customer123!",
            NewPassword = GeneratePasswordWithLength(1000),
            ConfirmPassword = GeneratePasswordWithLength(1000)
        };

        private static string GeneratePasswordWithLength(int length)
        {
            if (length < 8) return "Short1!";

            // Ensure we meet all requirements and then fill to desired length
            var basePassword = "Aa1!";
            var remainingLength = length - basePassword.Length;
            var filler = string.Join("", Enumerable.Repeat("a", remainingLength));
            return basePassword + filler;
        }
    }

    // Edge cases
    public static class EdgeCases
    {
        public static object CreateRequestWithUnicodeCharacters() => new
        {
            CurrentPassword = "Customer123!",
            NewPassword = "Påssw0rd123!ñ",
            ConfirmPassword = "Påssw0rd123!ñ"
        };

        public static object CreateRequestWithWhitespaceInPasswords() => new
        {
            CurrentPassword = "  CurrentPassword123!  ",
            NewPassword = "  NewPassword123!  ",
            ConfirmPassword = "  NewPassword123!  "
        };

        public static object CreateRequestWithOnlyWhitespace() => new
        {
            CurrentPassword = "   ",
            NewPassword = "   ",
            ConfirmPassword = "   "
        };

        public static object CreateRequestWithTabsAndNewlines() => new
        {
            CurrentPassword = "Customer123!",
            NewPassword = "New\tPassword\n123!",
            ConfirmPassword = "New\tPassword\n123!"
        };
    }

    // Security tests
    public static class SecurityTests
    {
        public static object CreateSqlInjectionAttempt() => new
        {
            CurrentPassword = "'; DROP TABLE Users; --",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        public static object CreateXssAttempt() => new
        {
            CurrentPassword = "<script>alert('xss')</script>",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        public static object CreateCommandInjectionAttempt() => new
        {
            CurrentPassword = "Customer123!",
            NewPassword = "$(rm -rf /)",
            ConfirmPassword = "$(rm -rf /)"
        };

        public static object CreateLongPasswordAttack() => new
        {
            CurrentPassword = new string('A', 10000),
            NewPassword = new string('B', 10000) + "1!",
            ConfirmPassword = new string('B', 10000) + "1!"
        };

        public static object CreateNullByteAttempt() => new
        {
            CurrentPassword = "CurrentPassword123!\0",
            NewPassword = "NewPassword123!\0",
            ConfirmPassword = "NewPassword123!\0"
        };
    }

    // Helper method to generate a valid password that meets all requirements
    private static string GenerateValidPassword()
    {
        // Generate a password that meets all requirements:
        // - At least 8 characters
        // - Contains uppercase letter
        // - Contains lowercase letter
        // - Contains number
        // - Contains special character
        var baseChars = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        var upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        var digits = "0123456789".ToCharArray();
        var special = "!@#$%^&*()_+-=[]{}|;:,.<>?".ToCharArray();

        var password = "";
        password += _faker.Random.ArrayElement(upperChars); // At least one uppercase
        password += _faker.Random.ArrayElement(baseChars);  // At least one lowercase
        password += _faker.Random.ArrayElement(digits);     // At least one digit
        password += _faker.Random.ArrayElement(special);    // At least one special char

        // Fill remaining length with random valid characters
        var allChars = (baseChars.Concat(upperChars).Concat(digits).Concat(special)).ToArray();
        var remainingLength = _faker.Random.Int(4, 8); // Make it 8-12 chars total
        for (int i = 0; i < remainingLength; i++)
        {
            password += _faker.Random.ArrayElement(allChars);
        }

        // Shuffle the password to avoid predictable patterns
        return new string(password.OrderBy(x => _faker.Random.Int()).ToArray());
    }
}