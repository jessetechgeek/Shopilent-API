using Bogus;

namespace Shopilent.API.IntegrationTests.Endpoints.Identity.ResendVerification.V1;

public static class ResendVerificationTestDataV1
{
    private static readonly Faker _faker = new();

    public static object CreateValidRequest(string? email = null)
    {
        return new
        {
            Email = email ?? _faker.Internet.Email()
        };
    }

    public static object CreateRequestWithEmptyEmail()
    {
        return new
        {
            Email = ""
        };
    }

    public static object CreateRequestWithNullEmail()
    {
        return new
        {
            Email = (string?)null
        };
    }

    public static object CreateRequestWithWhitespaceEmail()
    {
        return new
        {
            Email = "   "
        };
    }

    public static object CreateRequestWithInvalidEmailFormat(string invalidEmail = "invalid-email")
    {
        return new
        {
            Email = invalidEmail
        };
    }

    public static object CreateRequestWithExistingVerifiedEmail(string email = "verified@shopilent.com")
    {
        return new
        {
            Email = email
        };
    }

    public static object CreateRequestWithExistingUnverifiedEmail(string email = "unverified@shopilent.com")
    {
        return new
        {
            Email = email
        };
    }

    public static object CreateRequestWithNonExistentEmail(string email = "nonexistent@shopilent.com")
    {
        return new
        {
            Email = email
        };
    }

    public static class ValidationTests
    {
        public static object CreateRequestWithEmailMissingAtSymbol()
        {
            return new
            {
                Email = "userexample.com"
            };
        }

        public static object CreateRequestWithEmailMissingDomain()
        {
            return new
            {
                Email = "user@"
            };
        }

        public static object CreateRequestWithEmailMissingLocalPart()
        {
            return new
            {
                Email = "@example.com"
            };
        }

        public static object CreateRequestWithMultipleAtSymbols()
        {
            return new
            {
                Email = "user@@example.com"
            };
        }

        public static object CreateRequestWithInvalidDomainFormat()
        {
            return new
            {
                Email = "user@example."
            };
        }

        public static object CreateRequestWithSpecialCharactersInLocalPart()
        {
            return new
            {
                Email = "user+tag@example.com"
            };
        }

        public static object CreateRequestWithNumbersInEmail()
        {
            return new
            {
                Email = "user123@example123.com"
            };
        }

        public static object CreateRequestWithSubdomainEmail()
        {
            return new
            {
                Email = "user@mail.example.com"
            };
        }
    }

    public static class BoundaryTests
    {
        public static object CreateRequestWithMinimumValidEmail()
        {
            return new
            {
                Email = "a@b.co" // 6 characters - minimum valid email
            };
        }

        public static object CreateRequestWithLongValidEmail()
        {
            // Create a long but valid email (254 characters total)
            var longLocalPart = new string('a', 240);
            return new
            {
                Email = $"{longLocalPart}@example.com" // 240 + 12 = 252 chars
            };
        }

        public static object CreateRequestWithExtremelyLongEmail()
        {
            // Create email longer than 254 characters to trigger validation error
            var longLocalPart = new string('a', 245);
            return new
            {
                Email = $"{longLocalPart}@example.com" // 245 + 12 = 257 chars > 254
            };
        }

        public static object CreateRequestWithModeratelyLongEmail()
        {
            // Create email that's long but not extremely long (100 characters)
            var longLocalPart = new string('a', 85);
            return new
            {
                Email = $"{longLocalPart}@example.com" // 85 + 12 = 97 chars
            };
        }
    }

    public static class EdgeCases
    {
        public static object CreateRequestWithEmailContainingUnicodeCharacters()
        {
            return new
            {
                Email = "üser@example.com"
            };
        }

        public static object CreateRequestWithEmailContainingDots()
        {
            return new
            {
                Email = "user.name@example.com"
            };
        }

        public static object CreateRequestWithEmailContainingDashes()
        {
            return new
            {
                Email = "user-name@example-domain.com"
            };
        }

        public static object CreateRequestWithEmailContainingUnderscores()
        {
            return new
            {
                Email = "user_name@example.com"
            };
        }

        public static object CreateRequestWithMixedCaseEmail()
        {
            return new
            {
                Email = "User.Name@Example.COM"
            };
        }

        public static object CreateRequestWithLeadingTrailingSpaces()
        {
            return new
            {
                Email = "  user@example.com  "
            };
        }
    }
}