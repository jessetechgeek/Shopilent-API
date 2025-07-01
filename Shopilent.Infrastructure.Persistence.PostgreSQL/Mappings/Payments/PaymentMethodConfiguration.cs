using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopilent.Domain.Payments;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Mappings.Payments;

public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("payment_methods");
        
        builder.HasKey(pm => pm.Id);
        
        // Base entity properties
        builder.Property(pm => pm.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever()
            .IsRequired();
            
        builder.Property(pm => pm.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
            
        builder.Property(pm => pm.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
            
        // Auditable entity properties
        builder.Property(pm => pm.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid");
            
        builder.Property(pm => pm.ModifiedBy)
            .HasColumnName("modified_by")
            .HasColumnType("uuid");
            
        builder.Property(pm => pm.LastModified)
            .HasColumnName("last_modified")
            .HasColumnType("timestamp with time zone");
        
        // PaymentMethod specific properties
        builder.Property(pm => pm.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired();
            
        builder.Property(pm => pm.Type)
            .HasColumnName("type")
            .HasColumnType("varchar(50)")
            .HasConversion<string>()
            .IsRequired();
            
        builder.Property(pm => pm.Provider)
            .HasColumnName("provider")
            .HasColumnType("varchar(50)")
            .HasConversion<string>()
            .IsRequired();
            
        builder.Property(pm => pm.Token)
            .HasColumnName("token")
            .HasColumnType("varchar(255)")
            .IsRequired();
            
        builder.Property(pm => pm.DisplayName)
            .HasColumnName("display_name")
            .HasColumnType("varchar(255)")
            .IsRequired();
            
        builder.Property(pm => pm.CardBrand)
            .HasColumnName("card_brand")
            .HasColumnType("varchar(50)")
            .IsRequired(false);
            
        builder.Property(pm => pm.LastFourDigits)
            .HasColumnName("last_four_digits")
            .HasColumnType("varchar(4)")
            .IsRequired(false);
            
        builder.Property(pm => pm.ExpiryDate)
            .HasColumnName("expiry_date")
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);
            
        builder.Property(pm => pm.IsDefault)
            .HasColumnName("is_default")
            .HasColumnType("boolean")
            .HasDefaultValue(false)
            .IsRequired();
            
        builder.Property(pm => pm.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasDefaultValue(true)
            .IsRequired();
            
        // Metadata as JSON
        builder.Property(pm => pm.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions { WriteIndented = false }),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, new JsonSerializerOptions { })
            )
            .HasDefaultValue(new Dictionary<string, object>())
            .IsRequired();
            
        // Relationships
        builder.HasOne<Domain.Identity.User>()
            .WithMany()
            .HasForeignKey(pm => pm.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Indexes
        builder.HasIndex(pm => pm.UserId);
        builder.HasIndex(pm => pm.Token);
        builder.HasIndex(pm => new { pm.UserId, pm.IsDefault });
        builder.HasIndex(pm => pm.IsActive);
        
        // Optimistic Concurrency
        builder.Property(pm => pm.Version)
            .HasColumnName("version")
            .HasDefaultValue(0)
            .IsRowVersion();
    }
}