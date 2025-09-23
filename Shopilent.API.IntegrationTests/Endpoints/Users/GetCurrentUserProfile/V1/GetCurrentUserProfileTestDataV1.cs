using Bogus;
using Shopilent.Domain.Identity.Enums;

namespace Shopilent.API.IntegrationTests.Endpoints.Users.GetCurrentUserProfile.V1;

public static class GetCurrentUserProfileTestDataV1
{
    private static readonly Faker _faker = new();

    // Test user data for creating users with different roles
    public static object CreateUserData(
        string? email = null,
        string? firstName = null,
        string? lastName = null,
        UserRole role = UserRole.Customer,
        bool isActive = true)
    {
        return new
        {
            Email = email ?? _faker.Internet.Email(),
            Password = "Password123!",
            FirstName = firstName ?? _faker.Name.FirstName(),
            LastName = lastName ?? _faker.Name.LastName(),
            MiddleName = _faker.Name.FirstName(),
            Phone = _faker.Phone.PhoneNumber(),
            Role = role,
            IsActive = isActive,
            EmailVerified = true
        };
    }

    // Create test user data for specific scenarios
    public static object CreateAdminUserData() => new
    {
        Email = "test.admin@example.com",
        Password = "Password123!",
        FirstName = "Test",
        LastName = "Admin",
        MiddleName = "User",
        Phone = "+1234567890",
        Role = UserRole.Admin,
        IsActive = true,
        EmailVerified = true
    };

    public static object CreateCustomerUserData() => new
    {
        Email = "test.customer@example.com",
        Password = "Password123!",
        FirstName = "Test",
        LastName = "Customer",
        MiddleName = "",
        Phone = "+0987654321",
        Role = UserRole.Customer,
        IsActive = true,
        EmailVerified = true
    };

    public static object CreateManagerUserData() => new
    {
        Email = "test.manager@example.com",
        Password = "Password123!",
        FirstName = "Test",
        LastName = "Manager",
        MiddleName = "User",
        Phone = "+1122334455",
        Role = UserRole.Manager,
        IsActive = true,
        EmailVerified = true
    };

    // Edge cases
    public static class EdgeCases
    {
        public static object CreateUserWithUnicodeNames() => new
        {
            Email = "unicode@example.com",
            Password = "Password123!",
            FirstName = "Ünicöde",
            LastName = "Üser",
            MiddleName = "Tëst",
            Phone = "+1234567890",
            Role = UserRole.Customer,
            IsActive = true,
            EmailVerified = true
        };

        public static object CreateUserWithLongNames() => new
        {
            Email = "longnames@example.com",
            Password = "Password123!",
            FirstName = new string('A', 50),
            LastName = new string('B', 50),
            MiddleName = new string('C', 25),
            Phone = "+1234567890",
            Role = UserRole.Customer,
            IsActive = true,
            EmailVerified = true
        };

        public static object CreateInactiveUserData() => new
        {
            Email = "inactive@example.com",
            Password = "Password123!",
            FirstName = "Inactive",
            LastName = "User",
            MiddleName = "",
            Phone = "+1234567890",
            Role = UserRole.Customer,
            IsActive = false,
            EmailVerified = true
        };

        public static object CreateUnverifiedEmailUserData() => new
        {
            Email = "unverified@example.com",
            Password = "Password123!",
            FirstName = "Unverified",
            LastName = "User",
            MiddleName = "",
            Phone = "+1234567890",
            Role = UserRole.Customer,
            IsActive = true,
            EmailVerified = false
        };
    }

    // Security test data
    public static class SecurityTests
    {
        public static object CreateUserWithSpecialCharacters() => new
        {
            Email = "special@example.com",
            Password = "Password123!",
            FirstName = "John's",
            LastName = "O'Connor",
            MiddleName = "D'Angelo",
            Phone = "+1234567890",
            Role = UserRole.Customer,
            IsActive = true,
            EmailVerified = true
        };
    }

    // Boundary tests
    public static class BoundaryTests
    {
        public static object CreateUserWithMinimumValidData() => new
        {
            Email = "min@example.com",
            Password = "Password123!",
            FirstName = "A",
            LastName = "B",
            MiddleName = "",
            Phone = "",
            Role = UserRole.Customer,
            IsActive = true,
            EmailVerified = true
        };

        public static object CreateUserWithMaximumValidData() => new
        {
            Email = "maximum@example.com",
            Password = "Password123!",
            FirstName = new string('F', 100),
            LastName = new string('L', 100),
            MiddleName = new string('M', 50),
            Phone = "+1234567890123456",
            Role = UserRole.Customer,
            IsActive = true,
            EmailVerified = true
        };
    }
}