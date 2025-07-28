using Microsoft.EntityFrameworkCore;
using Shopilent.Domain.Audit;
using Shopilent.Domain.Catalog;
using Shopilent.Domain.Identity;
using Shopilent.Domain.Payments;
using Shopilent.Domain.Sales;
using Shopilent.Domain.Shipping;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Shopilent.Application.Abstractions.Events;
using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Events;
using Shopilent.Domain.Outbox;
using Attribute = Shopilent.Domain.Catalog.Attribute;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Identity domain
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    // Shipping domain
    public DbSet<Address> Addresses { get; set; }

    // Catalog domain
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Attribute> Attributes { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<ProductAttribute> ProductAttributes { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<VariantAttribute> VariantAttributes { get; set; }

    // Sales domain
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    // Payments domain
    public DbSet<PaymentMethod> PaymentMethods { get; set; }
    public DbSet<Payment> Payments { get; set; }

    // Audit domain
    public DbSet<AuditLog> AuditLogs { get; set; }

    // Outbox pattern
    public DbSet<OutboxMessage> OutboxMessages { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Ignore<DomainEvent>();

        // Apply all configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var events = ChangeTracker.Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // Update timestamps
        UpdateTimestamps();

        // Process domain events before saving (create outbox messages)
        if (events.Any())
        {
            var domainEventService = this.GetService<IDomainEventService>();

            if (domainEventService != null)
            {
                foreach (var domainEvent in events)
                {
                    // This will add outbox messages to the current context
                    await domainEventService.ProcessEventAsync(domainEvent, cancellationToken);
                }

                // Clear domain events
                foreach (var entity in ChangeTracker.Entries<AggregateRoot>()
                             .Select(e => e.Entity)
                             .Where(e => e.DomainEvents.Any()))
                {
                    entity.ClearDomainEvents();
                }
            }
        }

        // Save everything in a single transaction
        var result = await base.SaveChangesAsync(cancellationToken);

        return result;
    }

    public override int SaveChanges()
    {
        // Update timestamps before saving changes
        UpdateTimestamps();

        return SaveChangesAsync().GetAwaiter().GetResult();
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Domain.Common.Entity && (
                e.State == EntityState.Added || 
                e.State == EntityState.Modified));
                
        foreach (var entry in entries)
        {
            var now = DateTime.UtcNow;
            
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(Domain.Common.Entity.CreatedAt)).CurrentValue = now;
            }
            
            entry.Property(nameof(Domain.Common.Entity.UpdatedAt)).CurrentValue = now;
        }
    }
}