using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopilent.Domain.Catalog;
using Attribute = Shopilent.Domain.Catalog.Attribute;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Mappings.Catalog;

public class VariantAttributeConfiguration : IEntityTypeConfiguration<VariantAttribute>
{
    public void Configure(EntityTypeBuilder<VariantAttribute> builder)
    {
        builder.ToTable("variant_attributes");
        
        builder.HasKey(va => new { va.VariantId, va.AttributeId });
        
        // Entity properties
        builder.Property(va => va.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever();
            
        builder.Property(va => va.VariantId)
            .HasColumnName("variant_id")
            .HasColumnType("uuid")
            .IsRequired();
            
        builder.Property(va => va.AttributeId)
            .HasColumnName("attribute_id")
            .HasColumnType("uuid")
            .IsRequired();
            
        builder.Property(va => va.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
            
        builder.Property(va => va.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
            
        // Value as JSON
        builder.Property(a => a.Value)
            .HasColumnName("value")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions { WriteIndented = false }),
                v => v == null
                    ? new Dictionary<string, object>()
                    : JsonSerializer.Deserialize<Dictionary<string, object>>(v,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            Converters = { new JsonStringEnumConverter() }
                        }) ?? new Dictionary<string, object>()
            );
            
        builder.Property(va => va.Version)
            .HasColumnName("version")
            .HasDefaultValue(0)
            .IsConcurrencyToken();
        
        // Relationships
        builder.HasOne<ProductVariant>()
            .WithMany(pv => pv.VariantAttributes)
            .HasForeignKey(va => va.VariantId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne<Attribute>()
            .WithMany()
            .HasForeignKey(va => va.AttributeId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Indexes
        builder.HasIndex(va => va.VariantId);
        builder.HasIndex(va => va.AttributeId);
        builder.HasIndex(a => a.Value)
            .HasMethod("gin");
    }
}