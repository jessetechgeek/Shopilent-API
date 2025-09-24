using Bogus;
using Shopilent.Domain.Identity.Enums;

namespace Shopilent.API.IntegrationTests.Endpoints.Users.ChangeUserRole.V1;

public static class ChangeUserRoleTestDataV1
{
    private static readonly Faker _faker = new();

    // Core valid request generator
    public static object CreateValidRequest(UserRole? role = null)
    {
        return new
        {
            Role = role ?? _faker.Random.Enum<UserRole>()
        };
    }

    // Specific role requests
    public static object CreateAdminRoleRequest() => new
    {
        Role = UserRole.Admin
    };

    public static object CreateManagerRoleRequest() => new
    {
        Role = UserRole.Manager
    };

    public static object CreateCustomerRoleRequest() => new
    {
        Role = UserRole.Customer
    };

    // Invalid role requests (for testing enum validation)
    public static object CreateInvalidRoleRequest() => new
    {
        Role = 999 // Invalid enum value
    };

    public static object CreateNullRoleRequest() => new
    {
        Role = (UserRole?)null
    };

    // Edge cases
    public static class EdgeCases
    {
        public static object CreateRequestWithStringRole() => new
        {
            Role = "InvalidString" // This should fail enum binding
        };

        public static object CreateRequestWithNegativeRole() => new
        {
            Role = -1 // Invalid negative enum value
        };

        public static object CreateRequestWithLargeRole() => new
        {
            Role = int.MaxValue // Very large invalid enum value
        };
    }

    // Security tests
    public static class SecurityTests
    {
        public static object CreateSqlInjectionAttempt() => new
        {
            Role = "'; DROP TABLE Users; --"
        };

        public static object CreateXssAttempt() => new
        {
            Role = "<script>alert('xss')</script>"
        };

        public static object CreateCommandInjectionAttempt() => new
        {
            Role = "$(rm -rf /)"
        };
    }

    // Test helper methods
    public static UserRole GetRandomValidRole()
    {
        return _faker.Random.Enum<UserRole>();
    }

    public static UserRole GetDifferentRole(UserRole currentRole)
    {
        var allRoles = Enum.GetValues<UserRole>();
        var availableRoles = allRoles.Where(r => r != currentRole).ToArray();
        return _faker.Random.ArrayElement(availableRoles);
    }

    public static string GetRoleName(UserRole role)
    {
        return role switch
        {
            UserRole.Admin => "Admin",
            UserRole.Manager => "Manager", 
            UserRole.Customer => "Customer",
            _ => "Unknown"
        };
    }
}