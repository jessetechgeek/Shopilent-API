using Bogus;

namespace Shopilent.API.IntegrationTests.Endpoints.Users.UpdateUser.V1;

public static class UpdateUserTestDataV1
{
    private static readonly Faker _faker = new();

    // Core valid request generator
    public static object CreateValidRequest(
        string? firstName = null,
        string? lastName = null,
        string? middleName = null,
        string? phone = null)
    {
        return new
        {
            FirstName = firstName ?? _faker.Name.FirstName(),
            LastName = lastName ?? _faker.Name.LastName(),
            MiddleName = middleName ?? _faker.Name.FirstName(),
            Phone = phone ?? _faker.Phone.PhoneNumber("+1##########")
        };
    }

    // Required field validation test cases
    public static object CreateRequestWithEmptyFirstName() => new
    {
        FirstName = "",
        LastName = _faker.Name.LastName(),
        MiddleName = _faker.Name.FirstName(),
        Phone = _faker.Phone.PhoneNumber("+1##########")
    };

    public static object CreateRequestWithNullFirstName() => new
    {
        FirstName = (string)null,
        LastName = _faker.Name.LastName(),
        MiddleName = _faker.Name.FirstName(),
        Phone = _faker.Phone.PhoneNumber("+1##########")
    };

    public static object CreateRequestWithEmptyLastName() => new
    {
        FirstName = _faker.Name.FirstName(),
        LastName = "",
        MiddleName = _faker.Name.FirstName(),
        Phone = _faker.Phone.PhoneNumber("+1##########")
    };

    public static object CreateRequestWithNullLastName() => new
    {
        FirstName = _faker.Name.FirstName(),
        LastName = (string)null,
        MiddleName = _faker.Name.FirstName(),
        Phone = _faker.Phone.PhoneNumber("+1##########")
    };

    // Optional fields test cases
    public static object CreateRequestWithOnlyRequiredFields() => new
    {
        FirstName = _faker.Name.FirstName(),
        LastName = _faker.Name.LastName(),
        MiddleName = (string)null,
        Phone = (string)null
    };

    public static object CreateRequestWithEmptyOptionalFields() => new
    {
        FirstName = _faker.Name.FirstName(),
        LastName = _faker.Name.LastName(),
        MiddleName = "",
        Phone = ""
    };

    // Phone validation test cases
    public static object CreateRequestWithInvalidPhone() => new
    {
        FirstName = _faker.Name.FirstName(),
        LastName = _faker.Name.LastName(),
        MiddleName = _faker.Name.FirstName(),
        Phone = "invalid-phone"
    };

    public static object CreateRequestWithInvalidPhoneFormat() => new
    {
        FirstName = _faker.Name.FirstName(),
        LastName = _faker.Name.LastName(),
        MiddleName = _faker.Name.FirstName(),
        Phone = "0123456789" // Starts with 0, invalid per regex
    };

    // Boundary value tests
    public static class BoundaryTests
    {
        public static object CreateRequestWithMaximumLengthFirstName() => new
        {
            FirstName = new string('A', 50), // Maximum valid length
            LastName = _faker.Name.LastName(),
            MiddleName = _faker.Name.FirstName(),
            Phone = _faker.Phone.PhoneNumber("+1##########")
        };

        public static object CreateRequestWithMaximumLengthLastName() => new
        {
            FirstName = _faker.Name.FirstName(),
            LastName = new string('B', 50), // Maximum valid length
            MiddleName = _faker.Name.FirstName(),
            Phone = _faker.Phone.PhoneNumber("+1##########")
        };

        public static object CreateRequestWithMaximumLengthMiddleName() => new
        {
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            MiddleName = new string('C', 50), // Maximum valid length
            Phone = _faker.Phone.PhoneNumber("+1##########")
        };

        public static object CreateRequestWithTooLongFirstName() => new
        {
            FirstName = new string('A', 51), // Exceeds maximum
            LastName = _faker.Name.LastName(),
            MiddleName = _faker.Name.FirstName(),
            Phone = _faker.Phone.PhoneNumber("+1##########")
        };

        public static object CreateRequestWithTooLongLastName() => new
        {
            FirstName = _faker.Name.FirstName(),
            LastName = new string('B', 51), // Exceeds maximum
            MiddleName = _faker.Name.FirstName(),
            Phone = _faker.Phone.PhoneNumber("+1##########")
        };

        public static object CreateRequestWithTooLongMiddleName() => new
        {
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            MiddleName = new string('C', 51), // Exceeds maximum
            Phone = _faker.Phone.PhoneNumber("+1##########")
        };

        public static object CreateRequestWithMinimumValidPhone() => new
        {
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            MiddleName = _faker.Name.FirstName(),
            Phone = "+1234567" // Minimum valid: 7+ total characters per domain rules
        };

        public static object CreateRequestWithMaximumValidPhone() => new
        {
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            MiddleName = _faker.Name.FirstName(),
            Phone = "+123456789012345" // Maximum valid: 15 digits total
        };
    }

    // Edge cases
    public static class EdgeCases
    {
        public static object CreateRequestWithUnicodeCharacters() => new
        {
            FirstName = "Café",
            LastName = "Münchën",
            MiddleName = "José-María",
            Phone = "+34123456789"
        };

        public static object CreateRequestWithSpecialCharacters() => new
        {
            FirstName = "Mary-Jane",
            LastName = "O'Connor",
            MiddleName = "D'Angelo",
            Phone = "+1234567890"
        };

        public static object CreateRequestWithWhitespaceNames() => new
        {
            FirstName = "  John  ",
            LastName = "  Doe  ",
            MiddleName = "  Middle  ",
            Phone = "+1234567890"
        };

        public static object CreateRequestWithInternationalPhoneNumbers() => new
        {
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            MiddleName = _faker.Name.FirstName(),
            Phone = "+44123456789" // UK format
        };

        public static object CreateRequestWithPlusInPhone() => new
        {
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            MiddleName = _faker.Name.FirstName(),
            Phone = "1234567890" // Valid without + prefix
        };
    }

    // Valid test data variants
    public static class ValidRequestVariants
    {
        public static object CreateMinimalValidRequest() => new
        {
            FirstName = "John",
            LastName = "Doe",
            MiddleName = (string)null,
            Phone = (string)null
        };

        public static object CreateFullValidRequest() => new
        {
            FirstName = "John",
            LastName = "Doe",
            MiddleName = "Michael",
            Phone = "+1234567890"
        };

        public static object CreateRequestWithJustPhone() => new
        {
            FirstName = "John",
            LastName = "Doe",
            MiddleName = (string)null,
            Phone = "+1234567890"
        };

        public static object CreateRequestWithJustMiddleName() => new
        {
            FirstName = "John",
            LastName = "Doe",
            MiddleName = "Michael",
            Phone = (string)null
        };
    }
}