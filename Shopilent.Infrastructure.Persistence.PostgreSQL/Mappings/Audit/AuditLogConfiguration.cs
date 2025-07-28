using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopilent.Domain.Audit;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Mappings.Audit;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(al => al.Id);

        // Base entity properties
        builder.Property(al => al.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(al => al.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // AuditLog specific properties
        builder.Property(al => al.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid");

        builder.Property(al => al.EntityType)
            .HasColumnName("entity_type")
            .HasColumnType("varchar(100)")
            .IsRequired();

        builder.Property(al => al.EntityId)
            .HasColumnName("entity_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(al => al.Action)
            .HasColumnName("action")
            .HasColumnType("varchar(50)")
            .HasConversion<string>()
            .IsRequired();

        // OldValues and NewValues as JSON
        builder.Property(al => al.OldValues)
            .HasColumnName("old_values")
            .HasColumnType("jsonb")
            .HasConversion(
                v => v != null
                    ? JsonSerializer.Serialize(v, new JsonSerializerOptions { WriteIndented = false })
                    : null,
                v => v != null
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(v, new JsonSerializerOptions { })
                    : null
            );

        builder.Property(al => al.NewValues)
            .HasColumnName("new_values")
            .HasColumnType("jsonb")
            .HasConversion(
                v => v != null
                    ? JsonSerializer.Serialize(v, new JsonSerializerOptions { WriteIndented = false })
                    : null,
                v => v != null
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(v, new JsonSerializerOptions { })
                    : null
            );

        builder.Property(al => al.IpAddress)
            .HasColumnName("ip_address")
            .HasColumnType("varchar(45)");

        builder.Property(al => al.UserAgent)
            .HasColumnName("user_agent")
            .HasColumnType("text");

        builder.Property(al => al.AppVersion)
            .HasColumnName("app_version")
            .HasColumnType("varchar(50)")
            .IsRequired(false);

        builder.Property(al => al.Version)
            .HasColumnName("version")
            .HasDefaultValue(0)
            .IsConcurrencyToken();
        
        // Relationships
        builder.HasOne<Domain.Identity.User>()
            .WithMany()
            .HasForeignKey(al => al.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(al => al.UserId);
        builder.HasIndex(al => al.EntityType);
        builder.HasIndex(al => al.EntityId);
        builder.HasIndex(al => al.Action);
        builder.HasIndex(al => al.CreatedAt);
        builder.HasIndex(al => new { al.EntityType, al.EntityId, al.CreatedAt });
    }
}