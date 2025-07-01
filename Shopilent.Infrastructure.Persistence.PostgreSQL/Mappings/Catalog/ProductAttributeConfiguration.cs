using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopilent.Domain.Catalog;
using Attribute = Shopilent.Domain.Catalog.Attribute;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Mappings.Catalog;

public class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
{
    public void Configure(EntityTypeBuilder<ProductAttribute> builder)
    {
        builder.ToTable("product_attributes");

        builder.HasKey(pa => pa.Id);

        // Entity properties
        builder.Property(pa => pa.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(pa => pa.ProductId)
            .HasColumnName("product_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(pa => pa.AttributeId)
            .HasColumnName("attribute_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(pa => pa.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(pa => pa.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Values as JSON
        builder.Property(a => a.Values)
            .HasColumnName("values")
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

        builder.Property(pa => pa.Version)
            .HasColumnName("version")
            .HasDefaultValue(0)
            .IsRowVersion();

        // Relationships
        builder.HasOne<Product>()
            .WithMany(p => p.Attributes)
            .HasForeignKey(pa => pa.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Attribute>()
            .WithMany()
            .HasForeignKey(pa => pa.AttributeId)
            .OnDelete(DeleteBehavior.Cascade);


        // Unique constraint
        builder.HasIndex(pa => new { pa.ProductId, pa.AttributeId })
            .IsUnique();

        // Indexes
        builder.HasIndex(pa => pa.ProductId);
        builder.HasIndex(pa => pa.AttributeId);
        builder.HasIndex(a => a.Values)
            .HasMethod("gin");
    }
}