using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Attribute = Shopilent.Domain.Catalog.Attribute;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Mappings.Catalog;

public class AttributeConfiguration : IEntityTypeConfiguration<Attribute>
{
    public void Configure(EntityTypeBuilder<Attribute> builder)
    {
        builder.ToTable("attributes");

        builder.HasKey(a => a.Id);

        // Base entity properties
        builder.Property(a => a.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Auditable entity properties
        builder.Property(a => a.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid");

        builder.Property(a => a.ModifiedBy)
            .HasColumnName("modified_by")
            .HasColumnType("uuid");

        builder.Property(a => a.LastModified)
            .HasColumnName("last_modified")
            .HasColumnType("timestamp with time zone");

        // Attribute specific properties
        builder.Property(a => a.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(100)")
            .IsRequired();

        builder.Property(a => a.DisplayName)
            .HasColumnName("display_name")
            .HasColumnType("varchar(100)")
            .IsRequired();

        builder.Property(a => a.Type)
            .HasColumnName("type")
            .HasColumnType("varchar(50)")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(a => a.Filterable)
            .HasColumnName("filterable")
            .HasColumnType("boolean")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(a => a.Searchable)
            .HasColumnName("searchable")
            .HasColumnType("boolean")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(a => a.IsVariant)
            .HasColumnName("is_variant")
            .HasColumnType("boolean")
            .HasDefaultValue(false)
            .IsRequired();


        builder.Property(a => a.Configuration)
            .HasColumnName("configuration")
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
            ).IsRequired(false);

        // Indexes
        builder.HasIndex(a => a.Name)
            .IsUnique();
        builder.HasIndex(a => a.IsVariant);
        builder.HasIndex(a => a.Configuration)
            .HasMethod("gin");
        
        // Optimistic Concurrency
        builder.Property(a => a.Version)
            .HasColumnName("version")
            .HasDefaultValue(0)
            .IsConcurrencyToken();
    }
}