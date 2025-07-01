using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopilent.Domain.Outbox;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Mappings.Outbox;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(o => o.Id);

       builder.Property(o => o.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // OutboxMessage specific properties
        builder.Property(o => o.Type)
            .HasColumnName("type")
            .HasColumnType("varchar(500)")
            .IsRequired();

        builder.Property(o => o.Content)
            .HasColumnName("content")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(o => o.ProcessedAt)
            .HasColumnName("processed_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(o => o.Error)
            .HasColumnName("error")
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(o => o.RetryCount)
            .HasColumnName("retry_count")
            .HasColumnType("integer")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(o => o.ScheduledAt)
            .HasColumnName("scheduled_at")
            .HasColumnType("timestamp with time zone");

        // Indexes
        builder.HasIndex(o => o.ProcessedAt);
        builder.HasIndex(o => o.ScheduledAt);
    }
}