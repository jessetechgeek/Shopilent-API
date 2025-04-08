namespace Shopilent.Application.Common.Constants;

public enum AuthorizationPolicy
{
    RequireCustomer,
    RequireAdmin,
    RequireManager,
    RequireAdminOrManager,
    RequireAuthenticated
}