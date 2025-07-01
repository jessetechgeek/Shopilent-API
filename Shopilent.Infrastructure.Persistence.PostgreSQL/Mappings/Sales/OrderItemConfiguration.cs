using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopilent.Domain.Sales;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Mappings.Sales;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");
        
        builder.HasKey(oi => oi.Id);
        
        // Entity properties
        builder.Property(oi => oi.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever()
            .IsRequired();
            
        builder.Property(oi => oi.OrderId)
            .HasColumnName("order_id")
            .HasColumnType("uuid")
            .IsRequired();
            
        builder.Property(oi => oi.ProductId)
            .HasColumnName("product_id")
            .HasColumnType("uuid")
            .IsRequired();
            
        builder.Property(oi => oi.VariantId)
            .HasColumnName("variant_id")
            .HasColumnType("uuid");
            
        builder.Property(oi => oi.Quantity)
            .HasColumnName("quantity")
            .HasColumnType("integer")
            .IsRequired();
            
        builder.Property(oi => oi.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
            
        builder.Property(oi => oi.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
            
        // Money value objects
        builder.OwnsOne(oi => oi.UnitPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("unit_price")
                .HasColumnType("decimal(12, 2)")
                .IsRequired();
                
            money.Property(m => m.Currency)
                .HasColumnName("currency")
                .HasColumnType("varchar(3)")
                .HasDefaultValue("USD")
                .IsRequired();
        });
        
        builder.OwnsOne(oi => oi.TotalPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("total_price")
                .HasColumnType("decimal(12, 2)")
                .IsRequired();
                
            money.Property(m => m.Currency)
                .HasColumnName("total_price_currency")
                .HasColumnType("varchar(3)")
                .HasDefaultValue("USD")
                .IsRequired();
        });
        
        // Product data snapshot as JSON
        builder.Property(oi => oi.ProductData)
            .HasColumnName("product_data")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions { WriteIndented = false }),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, new JsonSerializerOptions { })
            )
            .IsRequired();
            
        // Relationships
        builder.HasOne<Order>()
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne<Domain.Catalog.Product>()
            .WithMany()
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne<Domain.Catalog.ProductVariant>()
            .WithMany()
            .HasForeignKey(oi => oi.VariantId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Indexes
        builder.HasIndex(oi => oi.OrderId);
        builder.HasIndex(oi => oi.ProductId);
        builder.HasIndex(oi => oi.VariantId);
        
        // Constraints
        builder.HasCheckConstraint("check_positive_order_quantity", "quantity > 0");
        builder.HasCheckConstraint("check_positive_unit_price", "unit_price >= 0");
        builder.HasCheckConstraint("check_positive_total_price", "total_price >= 0");
        
        // Optimistic Concurrency
        builder.Property(oi => oi.Version)
            .HasColumnName("version")
            .HasDefaultValue(0)
            .IsRowVersion();
    }
}