using Microsoft.Extensions.DependencyInjection;
using Shopilent.Application.Common.Constants;
using Shopilent.Domain.Identity.Enums;

namespace Shopilent.Infrastructure.Identity.Configuration.Extensions;

public static class AuthorizationPolicyExtensions
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Policy for regular customers
            options.AddPolicy(nameof(AuthorizationPolicy.RequireCustomer),
                policy => policy.RequireRole(nameof(UserRole.Customer)));

            // Policy for admin users
            options.AddPolicy(nameof(AuthorizationPolicy.RequireAdmin),
                policy => policy.RequireRole(nameof(UserRole.Admin)));

            // Policy for manager users
            options.AddPolicy(nameof(AuthorizationPolicy.RequireManager),
                policy => policy.RequireRole(nameof(UserRole.Manager)));

            // Policy for admin or manager users
            options.AddPolicy(nameof(AuthorizationPolicy.RequireAdminOrManager),
                policy => policy.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Manager)));

            // Policy just requiring authenticated users (any role)
            options.AddPolicy(nameof(AuthorizationPolicy.RequireAuthenticated),
                policy => policy.RequireAuthenticatedUser());
        });

        return services;
    }
}