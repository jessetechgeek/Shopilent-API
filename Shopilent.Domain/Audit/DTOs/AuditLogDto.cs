using Shopilent.Domain.Audit.Enums;

namespace Shopilent.Domain.Audit.DTOs;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string UserName { get; set; }
    public string UserEmail { get; set; }
    public string EntityType { get; set; }
    public Guid EntityId { get; set; }
    public AuditAction Action { get; set; }
    public Dictionary<string, object> OldValues { get; set; }
    public Dictionary<string, object> NewValues { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public string AppVersion { get; set; }
    public DateTime CreatedAt { get; set; }
}