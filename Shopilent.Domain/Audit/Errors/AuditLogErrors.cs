using Shopilent.Domain.Common.Errors;

namespace Shopilent.Domain.Audit.Errors;

public static class AuditLogErrors
{
    public static Error EntityTypeRequired => Error.Validation(
        code: "AuditLog.EntityTypeRequired",
        message: "Entity type cannot be empty.");

    public static Error InvalidEntityId => Error.Validation(
        code: "AuditLog.InvalidEntityId",
        message: "Entity ID cannot be empty or invalid.");

    public static Error NotFound(Guid id) => Error.NotFound(
        code: "AuditLog.NotFound",
        message: $"Audit log with ID {id} was not found.");
        
    public static Error InvalidAction => Error.Validation(
        code: "AuditLog.InvalidAction",
        message: "Invalid audit action specified.");
}