using Bogus;
using Shopilent.Domain.Identity.Enums;

namespace Shopilent.API.IntegrationTests.Endpoints.Users.GetUser.V1;

public static class GetUserTestDataV1
{
    private static readonly Faker _faker = new();

    // Valid user ID scenarios
    public static Guid CreateValidUserId() => Guid.NewGuid();

    public static Guid CreateExistingAdminUserId() => Guid.Parse("11111111-1111-1111-1111-111111111111"); // Predictable ID for test admin

    public static Guid CreateExistingCustomerUserId() => Guid.Parse("22222222-2222-2222-2222-222222222222"); // Predictable ID for test customer

    // User creation data for testing
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

    // Edge cases
    public static class EdgeCases
    {
        public static Guid CreateNonExistentUserId() => Guid.Parse("99999999-9999-9999-9999-999999999999");

        public static Guid CreateInvalidUserId() => Guid.Empty;

        public static string CreateMalformedUserId() => "not-a-guid";

        public static Guid CreateUserIdWithSpecialCharacters() => Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
    }

    // Security test data
    public static class SecurityTests
    {
        public static string CreateSqlInjectionUserId() => "'; DROP TABLE Users; --";

        public static string CreateXssUserId() => "<script>alert('xss')</script>";

        public static Guid CreateDeletedUserId() => Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    }

    // Boundary tests
    public static class BoundaryTests
    {
        public static Guid CreateMinimumValidGuid() => new Guid("00000000-0000-0000-0000-000000000001");

        public static Guid CreateMaximumValidGuid() => new Guid("ffffffff-ffff-ffff-ffff-fffffffffffe");
    }
}