using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopilent.Domain.Sales;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Mappings.Sales;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("cart_items");
        
        builder.HasKey(ci => ci.Id);
        
        // Entity properties
        builder.Property(ci => ci.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever()
            .IsRequired();
            
        builder.Property(ci => ci.CartId)
            .HasColumnName("cart_id")
            .HasColumnType("uuid")
            .IsRequired();
            
        builder.Property(ci => ci.ProductId)
            .HasColumnName("product_id")
            .HasColumnType("uuid")
            .IsRequired();
            
        builder.Property(ci => ci.VariantId)
            .HasColumnName("variant_id")
            .HasColumnType("uuid");
            
        builder.Property(ci => ci.Quantity)
            .HasColumnName("quantity")
            .HasColumnType("integer")
            .HasDefaultValue(1)
            .IsRequired();
            
        builder.Property(ci => ci.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
            
        builder.Property(ci => ci.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
            
        // Relationships
        builder.HasOne<Cart>()
            .WithMany(c => c.Items)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne<Domain.Catalog.Product>()
            .WithMany()
            .HasForeignKey(ci => ci.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne<Domain.Catalog.ProductVariant>()
            .WithMany()
            .HasForeignKey(ci => ci.VariantId)
            .OnDelete(DeleteBehavior.SetNull);
            
        // Indexes
        builder.HasIndex(ci => ci.CartId);
        builder.HasIndex(ci => ci.ProductId);
        builder.HasIndex(ci => ci.VariantId);
        
        // Constraints
        builder.HasCheckConstraint("check_positive_quantity", "quantity > 0");
        
        // Optimistic Concurrency
        builder.Property(ci => ci.Version)
            .HasColumnName("version")
            .HasDefaultValue(0)
            .IsConcurrencyToken();
    }
}