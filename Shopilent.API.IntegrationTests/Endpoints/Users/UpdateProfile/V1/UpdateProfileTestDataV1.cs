using Bogus;

namespace Shopilent.API.IntegrationTests.Endpoints.Users.UpdateProfile.V1;

public static class UpdateProfileTestDataV1
{
    private static readonly Faker _faker = new();

    public static object CreateValidRequest(
        string? firstName = null,
        string? lastName = null,
        string? middleName = null,
        string? phone = null)
    {
        // Use ASCII-only names that comply with validation pattern ^[a-zA-Z\s\-'\.]+$
        var validFirstNames = new[] { "John", "Jane", "Michael", "Sarah", "David", "Emily", "Robert", "Lisa" };
        var validLastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis" };
        var validMiddleNames = new[] { "James", "Marie", "Ann", "Lee", "Rose", "Grace", "Paul", "Lynn" };

        return new
        {
            FirstName = firstName ?? validFirstNames[_faker.Random.Int(0, validFirstNames.Length - 1)],
            LastName = lastName ?? validLastNames[_faker.Random.Int(0, validLastNames.Length - 1)],
            MiddleName = middleName ?? (_faker.Random.Bool(0.3f) ? validMiddleNames[_faker.Random.Int(0, validMiddleNames.Length - 1)] : null),
            Phone = phone ?? "1234567890"
        };
    }

    public static object CreateRequestWithEmptyFirstName() => new
    {
        FirstName = "",
        LastName = "Smith",
        MiddleName = "James",
        Phone = "1234567890"
    };

    public static object CreateRequestWithNullFirstName() => new
    {
        FirstName = (string?)null,
        LastName = "Johnson",
        MiddleName = "Marie",
        Phone = "1234567890"
    };

    public static object CreateRequestWithEmptyLastName() => new
    {
        FirstName = "John",
        LastName = "",
        MiddleName = "James",
        Phone = "1234567890"
    };

    public static object CreateRequestWithNullLastName() => new
    {
        FirstName = "Jane",
        LastName = (string?)null,
        MiddleName = "Ann",
        Phone = "1234567890"
    };

    public static object CreateRequestWithNullMiddleName() => new
    {
        FirstName = "Michael",
        LastName = "Williams",
        MiddleName = (string?)null,
        Phone = "1234567890"
    };

    public static object CreateRequestWithEmptyMiddleName() => new
    {
        FirstName = "Sarah",
        LastName = "Brown",
        MiddleName = "",
        Phone = "1234567890"
    };

    public static object CreateRequestWithNullPhone() => new
    {
        FirstName = "David",
        LastName = "Jones",
        MiddleName = "Lee",
        Phone = (string?)null
    };

    public static object CreateRequestWithEmptyPhone() => new
    {
        FirstName = "Emily",
        LastName = "Miller",
        MiddleName = "Rose",
        Phone = ""
    };

    public static object CreateRequestWithInvalidPhone() => new
    {
        FirstName = "Robert",
        LastName = "Davis",
        MiddleName = "Grace",
        Phone = "invalid-phone"
    };

    public static object CreateRequestWithValidInternationalPhone() => new
    {
        FirstName = "Lisa",
        LastName = "Wilson",
        MiddleName = "Paul",
        Phone = "+1234567890123"
    };

    public static object CreateRequestWithInvalidCharactersInName() => new
    {
        FirstName = "John123@",
        LastName = "Doe456#",
        MiddleName = "Test$%",
        Phone = "1234567890"
    };

    // Boundary value testing
    public static class BoundaryTests
    {
        public static object CreateRequestWithSingleCharacterName() => new
        {
            FirstName = "A", LastName = "B", MiddleName = "C", Phone = "1234567890"
        };

        public static object CreateRequestWithMaximumLengthNames() => new
        {
            FirstName = new string('A', 50), // Actual limit is 50
            LastName = new string('B', 50),
            MiddleName = new string('C', 50),
            Phone = "1234567890"
        };

        public static object CreateRequestWithLongNames() => new
        {
            FirstName = new string('A', 51), // Over limit
            LastName = new string('B', 51),
            MiddleName = new string('C', 51),
            Phone = "1234567890"
        };
    }

    // Edge cases
    public static class EdgeCases
    {
        public static object CreateRequestWithUnicodeCharacters() => new
        {
            FirstName = "José",
            LastName = "García-Martínez",
            MiddleName = "María",
            Phone = "1234567890"
        };

        public static object CreateRequestWithSpecialCharacters() => new
        {
            FirstName = "John-Paul",
            LastName = "O'Connor",
            MiddleName = "De'Angelo",
            Phone = "1234567890"
        };

        public static object CreateRequestWithWhitespaceInNames() => new
        {
            FirstName = " John ",
            LastName = " Doe ",
            MiddleName = " Middle ",
            Phone = "1234567890"
        };

        public static object CreateRequestWithOnlyWhitespace() => new
        {
            FirstName = "   ", LastName = "   ", MiddleName = "   ", Phone = "   "
        };

        public static object CreateRequestWithTabsAndNewlines() => new
        {
            FirstName = "John\t",
            LastName = "Doe\n",
            MiddleName = "Middle\r",
            Phone = "1234567890"
        };

        public static object CreateRequestWithNumericNames() => new
        {
            FirstName = "John123",
            LastName = "Doe456",
            MiddleName = "789",
            Phone = "1234567890"
        };

        public static object CreateRequestWithEmojis() => new
        {
            FirstName = "John😊",
            LastName = "Doe🎉",
            MiddleName = "Test👍",
            Phone = "1234567890"
        };

        public static object CreateRequestWithValidSpecialCharacters() => new
        {
            FirstName = "Mary-Jane",
            LastName = "O'Connor",
            MiddleName = "Ann-Marie",
            Phone = "1234567890"
        };

        public static object CreateRequestWithValidDotsAndSpaces() => new
        {
            FirstName = "John Jr.",
            LastName = "Van Der Berg",
            MiddleName = "De La Cruz",
            Phone = "1234567890"
        };
    }

    // Security tests
    public static class SecurityTests
    {
        public static object CreateSqlInjectionAttempt() => new
        {
            FirstName = "'; DROP TABLE Users; --",
            LastName = "'; SELECT * FROM Users; --",
            MiddleName = "'; UPDATE Users SET Role = 'Admin'; --",
            Phone = "1234567890"
        };

        public static object CreateXssAttempt() => new
        {
            FirstName = "<script>alert('xss')</script>",
            LastName = "<img src=x onerror=alert('xss')>",
            MiddleName = "javascript:alert('xss')",
            Phone = "1234567890"
        };

        public static object CreateCommandInjectionAttempt() => new
        {
            FirstName = "John; rm -rf /",
            LastName = "Doe | cat /etc/passwd",
            MiddleName = "Test && whoami",
            Phone = "1234567890"
        };

        public static object CreateLongStringAttack() => new
        {
            FirstName = new string('A', 10000),
            LastName = new string('B', 10000),
            MiddleName = new string('C', 10000),
            Phone = new string('1', 50)
        };

        public static object CreateNullByteAttempt() => new
        {
            FirstName = "John\0Admin",
            LastName = "Doe\0Root",
            MiddleName = "Test\0System",
            Phone = "1234567890"
        };
    }
}
