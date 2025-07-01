using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopilent.Domain.Catalog;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Mappings.Catalog;

public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("product_categories");
        
        builder.HasKey(pc => new { pc.ProductId, pc.CategoryId });
        
        // Entity properties
        builder.Property(pc => pc.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever();
            
        builder.Property(pc => pc.ProductId)
            .HasColumnName("product_id")
            .HasColumnType("uuid")
            .IsRequired();
            
        builder.Property(pc => pc.CategoryId)
            .HasColumnName("category_id")
            .HasColumnType("uuid")
            .IsRequired();
            
        builder.Property(pc => pc.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
            
        builder.Property(pc => pc.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        
        // Relationships
        builder.HasOne<Product>()
            .WithMany(p => p.Categories)
            .HasForeignKey(pc => pc.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne<Category>()
            .WithMany(c => c.ProductCategories)
            .HasForeignKey(pc => pc.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.Property(c => c.Version)
            .HasColumnName("version")
            .HasDefaultValue(0)
            .IsRowVersion();
        
        // Indexes
        builder.HasIndex(pc => pc.ProductId);
        builder.HasIndex(pc => pc.CategoryId);
    }
}