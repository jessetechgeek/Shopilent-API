using Bogus;

namespace Shopilent.API.IntegrationTests.Endpoints.Identity.Register.V1;

public static class RegisterTestDataV1
{
    private static readonly Faker _faker = new();

    public static object CreateValidRequest(
        string? email = null,
        string? password = null,
        string? firstName = null,
        string? lastName = null)
    {
        return new
        {
            Email = email ?? _faker.Internet.Email(),
            Password = password ?? "ValidPassword123!",
            FirstName = firstName ?? _faker.Name.FirstName(),
            LastName = lastName ?? _faker.Name.LastName()
        };
    }

    public static object CreateRequestWithInvalidEmail(string invalidEmail = "invalid-email")
    {
        return new
        {
            Email = invalidEmail,
            Password = "ValidPassword123!",
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName()
        };
    }

    public static object CreateRequestWithWeakPassword(string weakPassword = "weak")
    {
        return new
        {
            Email = _faker.Internet.Email(),
            Password = weakPassword,
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName()
        };
    }

    public static object CreateRequestWithEmptyFirstName()
    {
        return new
        {
            Email = _faker.Internet.Email(),
            Password = "ValidPassword123!",
            FirstName = "",
            LastName = _faker.Name.LastName()
        };
    }

    public static object CreateRequestWithEmptyLastName()
    {
        return new
        {
            Email = _faker.Internet.Email(),
            Password = "ValidPassword123!",
            FirstName = _faker.Name.FirstName(),
            LastName = ""
        };
    }

    public static object CreateRequestWithMissingEmail()
    {
        return new
        {
            Password = "ValidPassword123!",
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName()
        };
    }

    public static object CreateRequestWithMissingPassword()
    {
        return new
        {
            Email = _faker.Internet.Email(),
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName()
        };
    }

    public static object CreateRequestWithLongNames(int nameLength = 101)
    {
        // Create names longer than 100 characters to trigger validation error
        var longName = new string('A', nameLength);
        return new
        {
            Email = _faker.Internet.Email(),
            Password = "ValidPassword123!",
            FirstName = longName,
            LastName = longName
        };
    }

    public static object CreateRequestWithSpecialCharactersInNames()
    {
        return new
        {
            Email = _faker.Internet.Email(),
            Password = "ValidPassword123!",
            FirstName = "José",
            LastName = "O'Connor"
        };
    }

    public static object CreateRequestWithNullFirstName()
    {
        return new
        {
            Email = _faker.Internet.Email(),
            Password = "ValidPassword123!",
            FirstName = (string?)null,
            LastName = _faker.Name.LastName()
        };
    }

    public static object CreateRequestWithNullLastName()
    {
        return new
        {
            Email = _faker.Internet.Email(),
            Password = "ValidPassword123!",
            FirstName = _faker.Name.FirstName(),
            LastName = (string?)null
        };
    }

    public static object CreateRequestWithWhitespaceNames()
    {
        return new
        {
            Email = _faker.Internet.Email(),
            Password = "ValidPassword123!",
            FirstName = "   ",
            LastName = "   "
        };
    }

    public static object CreateRequestWithNumericNames()
    {
        return new
        {
            Email = _faker.Internet.Email(),
            Password = "ValidPassword123!",
            FirstName = "123",
            LastName = "456"
        };
    }

    public static object CreateRequestWithSpecialPassword(string specialPassword)
    {
        return new
        {
            Email = _faker.Internet.Email(),
            Password = specialPassword,
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName()
        };
    }

    public static object CreateRequestWithExtremelyLongEmail()
    {
        // Create email longer than 255 characters to trigger validation error
        var longLocalPart = new string('a', 245);
        var longEmail = $"{longLocalPart}@example.com"; // Total = 245 + 12 = 257 chars > 255

        return new
        {
            Email = longEmail,
            Password = "ValidPassword123!",
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName()
        };
    }

    public static object CreateRequestWithMultipleAtSymbolsInEmail()
    {
        return new
        {
            Email = "user@@example@.com",
            Password = "ValidPassword123!",
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName()
        };
    }

    public static object CreateRequestWithEmailMissingDomain()
    {
        return new
        {
            Email = "user@",
            Password = "ValidPassword123!",
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName()
        };
    }

    // Boundary values for password validation
    public static class PasswordTests
    {
        public static object CreateRequestWithMinimumValidPassword()
        {
            return new
            {
                Email = _faker.Internet.Email(),
                Password = "Password1!", // 10 chars: meets 8+ requirement, upper, lower, digit, special
                FirstName = _faker.Name.FirstName(),
                LastName = _faker.Name.LastName()
            };
        }

        public static object CreateRequestWithPasswordMissingUppercase()
        {
            return new
            {
                Email = _faker.Internet.Email(),
                Password = "password123!",
                FirstName = _faker.Name.FirstName(),
                LastName = _faker.Name.LastName()
            };
        }

        public static object CreateRequestWithPasswordMissingLowercase()
        {
            return new
            {
                Email = _faker.Internet.Email(),
                Password = "PASSWORD123!",
                FirstName = _faker.Name.FirstName(),
                LastName = _faker.Name.LastName()
            };
        }

        public static object CreateRequestWithPasswordMissingDigit()
        {
            return new
            {
                Email = _faker.Internet.Email(),
                Password = "Password!",
                FirstName = _faker.Name.FirstName(),
                LastName = _faker.Name.LastName()
            };
        }

        public static object CreateRequestWithPasswordMissingSpecialChar()
        {
            return new
            {
                Email = _faker.Internet.Email(),
                Password = "Password123",
                FirstName = _faker.Name.FirstName(),
                LastName = _faker.Name.LastName()
            };
        }

        public static object CreateRequestWithTooShortPassword()
        {
            return new
            {
                Email = _faker.Internet.Email(),
                Password = "P1!",
                FirstName = _faker.Name.FirstName(),
                LastName = _faker.Name.LastName()
            };
        }

        public static object CreateRequestWithExtremelyLongPassword()
        {
            var longPassword = new string('A', 252) + "1a!"; // Total = 252 + 3 = 255, so make it 253 + 3 = 256
            return new
            {
                Email = _faker.Internet.Email(),
                Password = new string('A', 253) + "1a!", // 256 chars > 255
                FirstName = _faker.Name.FirstName(),
                LastName = _faker.Name.LastName()
            };
        }
    }

    // Edge cases for names
    public static class NameTests
    {
        public static object CreateRequestWithSingleCharacterNames()
        {
            return new
            {
                Email = _faker.Internet.Email(),
                Password = "ValidPassword123!",
                FirstName = "A",
                LastName = "B"
            };
        }

        public static object CreateRequestWithHyphenatedNames()
        {
            return new
            {
                Email = _faker.Internet.Email(),
                Password = "ValidPassword123!",
                FirstName = "Mary-Jane",
                LastName = "Smith-Wilson"
            };
        }

        public static object CreateRequestWithNamesWithSpaces()
        {
            return new
            {
                Email = _faker.Internet.Email(),
                Password = "ValidPassword123!",
                FirstName = "Van Der",
                LastName = "Van Berg"
            };
        }

        public static object CreateRequestWithUnicodeNames()
        {
            return new
            {
                Email = _faker.Internet.Email(),
                Password = "ValidPassword123!",
                FirstName = "Björk",
                LastName = "Müller"
            };
        }
    }
}
