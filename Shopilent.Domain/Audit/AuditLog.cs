using Shopilent.Domain.Audit.Enums;
using Shopilent.Domain.Common;
using Shopilent.Domain.Identity;

namespace Shopilent.Domain.Audit;

public class AuditLog : Entity
{
    private AuditLog()
    {
        // Required by EF Core
    }

    private AuditLog(
        string entityType,
        Guid entityId,
        AuditAction action,
        User user = null,
        Dictionary<string, object> oldValues = null,
        Dictionary<string, object> newValues = null,
        string ipAddress = null,
        string userAgent = null,
        string appVersion = null)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type cannot be empty", nameof(entityType));

        EntityType = entityType;
        EntityId = entityId;
        Action = action;
        UserId = user?.Id;
        OldValues = oldValues;
        NewValues = newValues;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        AppVersion = appVersion;
    }

    public static AuditLog Create(
        string entityType,
        Guid entityId,
        AuditAction action,
        User user = null,
        Dictionary<string, object> oldValues = null,
        Dictionary<string, object> newValues = null,
        string ipAddress = null,
        string userAgent = null,
        string appVersion = null)
    {
        return new AuditLog(entityType, entityId, action, user, oldValues, newValues, ipAddress, userAgent, appVersion);
    }

    public static AuditLog CreateForCreate(
        string entityType,
        Guid entityId,
        Dictionary<string, object> values,
        User user = null,
        string ipAddress = null,
        string userAgent = null,
        string appVersion = null)
    {
        return new AuditLog(entityType, entityId, AuditAction.Create, user, null, values, ipAddress, userAgent,
            appVersion);
    }

    public static AuditLog CreateForUpdate(
        string entityType,
        Guid entityId,
        Dictionary<string, object> oldValues,
        Dictionary<string, object> newValues,
        User user = null,
        string ipAddress = null,
        string userAgent = null,
        string appVersion = null)
    {
        return new AuditLog(entityType, entityId, AuditAction.Update, user, oldValues, newValues, ipAddress, userAgent,
            appVersion);
    }

    public static AuditLog CreateForDelete(
        string entityType,
        Guid entityId,
        Dictionary<string, object> values,
        User user = null,
        string ipAddress = null,
        string userAgent = null,
        string appVersion = null)
    {
        return new AuditLog(entityType, entityId, AuditAction.Delete, user, values, null, ipAddress, userAgent,
            appVersion);
    }

    public Guid? UserId { get; private set; }
    public string EntityType { get; private set; }
    public Guid EntityId { get; private set; }
    public AuditAction Action { get; private set; }
    public Dictionary<string, object> OldValues { get; private set; }
    public Dictionary<string, object> NewValues { get; private set; }
    public string IpAddress { get; private set; }
    public string UserAgent { get; private set; }
    public string AppVersion { get; private set; }
}