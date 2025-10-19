namespace Shopilent.Domain.Common;

public abstract class AuditableEntity : Entity
{
    public Guid? CreatedBy { get; protected set; }
    public Guid? ModifiedBy { get; protected set; }
    public DateTime? LastModified { get; protected set; }

    public void SetAuditInfo(Guid? modifiedBy)
    {
        ModifiedBy = modifiedBy;
        LastModified = DateTime.UtcNow;
    }

    public void SetCreationAuditInfo(Guid? createdBy)
    {
        if (CreatedBy == null)
        {
            CreatedBy = createdBy;
            ModifiedBy = createdBy;
        }
    }
}
